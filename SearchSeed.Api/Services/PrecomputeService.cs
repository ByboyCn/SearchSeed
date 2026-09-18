using SearchSeed.Core;

namespace SearchSeed.Api.Services;

// 启动即开始的预热计算服务：按优先级顺序无限遍历种子空间，
// 已算过的（含用户查询过的）自动跳过；结果持久化到 GalaxyStore
public class PrecomputeService : BackgroundService
{
    readonly GalaxyStore _store;
    readonly SeedService _seeds;
    readonly ILogger<PrecomputeService> _log;
    readonly bool _enabled;
    static readonly SemaphoreSlim Gate = new(Math.Max(1, Environment.ProcessorCount / 2));

    // 预热顺序：64星/1x 优先（最常用），其次 32 星，其余组合靠后
    static readonly (int starNum, int resIdx)[] Priority =
    [
        (64, 4), (64, 5), (64, 3), (64, 6), (64, 7), (64, 8), (64, 2), (64, 1), (64, 0), (64, 9), (64, 10),
        (32, 4),
    ];

    public PrecomputeService(GalaxyStore store, SeedService seeds, IConfiguration cfg, ILogger<PrecomputeService> log)
    {
        _store = store; _seeds = seeds; _log = log;
        _enabled = cfg.GetValue("Precompute:Enabled", false);
    }

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        if (!_enabled)
        {
            _log.LogInformation("预热计算未启用 (Precompute:Enabled=false)");
            return;
        }
        _log.LogInformation("预热计算启动：顺序 {Order}", string.Join(",", Priority.Select(p => $"{p.starNum}星{p.resIdx}")));
        var progress = _store.LoadProgress();
        var tasks = Priority.Select(p => Task.Run(() => RunCombo(p.starNum, p.resIdx, progress, ct), ct)).ToArray();
        await Task.WhenAll(tasks);
    }

    async Task RunCombo(int starNum, int resIdx, Dictionary<string, ulong> progress, CancellationToken ct)
    {
        var key = $"{starNum}_{resIdx}_std";
        ulong cursor;
        lock (_store.ProgressLock) progress.TryGetValue(key, out cursor);
        int done = 0;
        while (!ct.IsCancellationRequested)
        {
            if (_store.IsDone((int)cursor, starNum, resIdx, false))
            {
                cursor++; continue;
            }
            await Gate.WaitAsync(ct);
            try
            {
                // 每算 16 个存一次游标
                var result = _seeds.GetGalaxy((int)cursor, starNum, resIdx, fastMode: false);
                _store.Save((int)cursor, starNum, resIdx, false, result);
                cursor++; done++;
                if (done % 16 == 0)
                {
                    lock (_store.ProgressLock) progress[key] = cursor;
                    _store.SaveProgress(progress);
                    _log.LogInformation("预热进度 {Key}: 下一种子 {Cursor} (已完成 {Done})", key, cursor, done);
                }
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "预热失败 {Key} seed={Cursor}", key, cursor);
                cursor++;
            }
            finally { Gate.Release(); }
            await Task.Delay(10, ct); // 让出 CPU 给用户请求
        }
    }
}
