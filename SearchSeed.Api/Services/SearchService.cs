using System.Collections.Concurrent;
using SearchSeed.Core;

namespace SearchSeed.Api.Services;

public class SearchService
{
    public readonly ConcurrentDictionary<string, SearchJob> Jobs = new();
    readonly GalaxyStore _store;
    readonly SeedService _seeds;
    readonly ILogger<SearchService> _log;

    readonly int _maxDegree;

    public SearchService(GalaxyStore store, SeedService seeds, IConfiguration cfg, ILogger<SearchService> log)
    {
        _store = store; _seeds = seeds; _log = log;
        _maxDegree = Math.Max(1, cfg.GetValue("Search:MaxConcurrency", Environment.ProcessorCount));
        _log.LogInformation("搜索并行度: {D} (CPU {N} 核)", _maxDegree, Environment.ProcessorCount);
    }

    public SearchJob Start(GalaxyCond cond, long fromSeed, int starNumFrom, int starNumTo, int resIdx, bool fast, int maxResults)
    {
        var job = new SearchJob
        {
            Conditions = cond, FromSeed = fromSeed,
            StarNumFrom = Math.Max(32, starNumFrom), StarNumTo = Math.Min(64, Math.Max(starNumFrom, starNumTo)),
            ResourceIndex = resIdx, FastMode = fast, MaxResults = maxResults,
        };
        Jobs[job.Id] = job;
        _ = Task.Run(() => Run(job));
        return job;
    }

    void Run(SearchJob job)
    {
        _log.LogInformation("搜索 {Id} 启动 from={From} starNum={A}~{B} fast={Fast}", job.Id, job.FromSeed, job.StarNumFrom, job.StarNumTo, job.FastMode);
        try
        {
            const int batch = 128;
            for (long start = job.FromSeed; !job.Stopped && start <= int.MaxValue; start += batch)
            {
                if (job.Matches.Count >= job.MaxResults) break;
                var range = new List<long>();
                for (long s = start; s < start + batch && s <= int.MaxValue; s++) range.Add(s);
                // 全核并行：多少核心跑多少线程（可被 Search:MaxConcurrency 覆盖）
                Parallel.ForEach(range, new ParallelOptions { MaxDegreeOfParallelism = _maxDegree }, seed =>
                {
                    if (job.Stopped || job.Matches.Count >= job.MaxResults) return;
                    for (int sn = job.StarNumFrom; sn <= job.StarNumTo; sn++)
                    {
                        if (job.Stopped || job.Matches.Count >= job.MaxResults) return;
                        try
                        {
                            bool cached = _store.IsDone((int)seed, sn, job.ResourceIndex, job.FastMode);
                            var g = _seeds.GetGalaxy((int)seed, sn, job.ResourceIndex, job.FastMode);
                            Interlocked.Increment(ref job.Scanned);
                            if (cached) Interlocked.Increment(ref job.Skipped);
                            if (CondEval.Check(g, job.Conditions, out var hits))
                            {
                                job.Matches.Enqueue(new SearchMatch
                                {
                                    Seed = (int)seed, StarNum = sn,
                                    Stars = hits.Select(s => new MatchedStar
                                    {
                                        Index = s.Index, Name = s.Name, Type = s.Type,
                                        Distance = s.Distance, DysonLumino = s.DysonLumino,
                                    }).ToList(),
                                });
                                _log.LogInformation("搜索 {Id} 命中 seed={Seed} starNum={N}", job.Id, seed, sn);
                            }
                        }
                        catch (Exception ex) { _log.LogError(ex, "搜索 {Id} seed={Seed} sn={N} 异常", job.Id, seed, sn); }
                    }
                });
            }
        }
        catch (Exception ex) { _log.LogError(ex, "搜索 {Id} 异常", job.Id); }
        job.Finished = true;
        _log.LogInformation("搜索 {Id} 结束 scanned={S} matches={M}", job.Id, job.Scanned, job.Matches.Count);
    }
}