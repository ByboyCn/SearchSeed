using System.Collections.Concurrent;
using SearchSeed.Core;

namespace SearchSeed.Api.Services;

public class SearchService
{
    public readonly ConcurrentDictionary<string, SearchJob> Jobs = new();
    readonly GalaxyStore _store;
    readonly SeedService _seeds;
    readonly ILogger<SearchService> _log;

    public SearchService(GalaxyStore store, SeedService seeds, ILogger<SearchService> log)
    {
        _store = store; _seeds = seeds; _log = log;
    }

    public SearchJob Start(GalaxyCond cond, long fromSeed, int starNum, int resIdx, bool fast, int maxResults)
    {
        var job = new SearchJob
        {
            Conditions = cond, FromSeed = fromSeed, StarNum = starNum,
            ResourceIndex = resIdx, FastMode = fast, MaxResults = maxResults,
        };
        Jobs[job.Id] = job;
        _ = Task.Run(() => Run(job));
        return job;
    }

    void Run(SearchJob job)
    {
        _log.LogInformation("搜索 {Id} 启动 from={From} starNum={N} fast={Fast}", job.Id, job.FromSeed, job.StarNum, job.FastMode);
        try
        {
            const int batch = 128;
            for (long start = job.FromSeed; !job.Stopped && start <= int.MaxValue; start += batch)
            {
                if (job.Matches.Count >= job.MaxResults) break;
                var range = new List<long>();
                for (long s = start; s < start + batch && s <= int.MaxValue; s++) range.Add(s);
                Parallel.ForEach(range, new ParallelOptions { MaxDegreeOfParallelism = 4 }, seed =>
                {
                    if (job.Stopped || job.Matches.Count >= job.MaxResults) return;
                    try
                    {
                        bool cached = _store.IsDone((int)seed, job.StarNum, job.ResourceIndex, job.FastMode);
                        var g = _seeds.GetGalaxy((int)seed, job.StarNum, job.ResourceIndex, job.FastMode);
                        Interlocked.Increment(ref job.Scanned);
                        if (cached) Interlocked.Increment(ref job.Skipped);
                        if (CondEval.Check(g, job.Conditions, out var hits))
                        {
                            job.Matches.Enqueue(new SearchMatch
                            {
                                Seed = (int)seed,
                                Stars = hits.Select(s => new MatchedStar
                                {
                                    Index = s.Index, Name = s.Name, Type = s.Type,
                                    Distance = s.Distance, DysonLumino = s.DysonLumino,
                                }).ToList(),
                            });
                            _log.LogInformation("搜索 {Id} 命中 seed={Seed}", job.Id, seed);
                        }
                    }
                    catch (Exception ex) { _log.LogError(ex, "搜索 {Id} seed={Seed} 异常", job.Id, seed); }
                });
            }
        }
        catch (Exception ex) { _log.LogError(ex, "搜索 {Id} 异常", job.Id); }
        job.Finished = true;
        _log.LogInformation("搜索 {Id} 结束 scanned={S} matches={M}", job.Id, job.Scanned, job.Matches.Count);
    }
}
