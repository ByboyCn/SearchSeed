using SearchSeed.Core;

namespace SearchSeed.Api.Services;

// 预热服务（轻量版）：空闲时单线程、只跑 64 星；资源倍率跟随用户最近使用（默认无限）
public class PrecomputeService : BackgroundService
{
    readonly GalaxyStore _store;
    readonly SeedService _seeds;
    readonly ILogger<PrecomputeService> _log;
    readonly bool _enabled;

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
        _log.LogInformation("预热启动（单线程，64 星，资源跟随用户，默认无限）");
        var progress = _store.LoadProgress();
        while (!ct.IsCancellationRequested)
        {
            int starNum = 64;
            int resIdx = _seeds.LastUsedResourceIndex;
            var key = $"{starNum}_{resIdx}_std";
            ulong cursor;
            lock (_store.ProgressLock) progress.TryGetValue(key, out cursor);
            int saved = 0;
            try
            {
                while (!ct.IsCancellationRequested)
                {
                    if (_seeds.LastUsedResourceIndex != resIdx) break; // 用户切换资源 → 转向新组合
                    if (_store.IsDone((int)cursor, starNum, resIdx, false)) { cursor++; continue; }
                    var result = _seeds.GetGalaxy((int)cursor, starNum, resIdx, fastMode: false);
                    _store.Save((int)cursor, starNum, resIdx, false, result);
                    cursor++; saved++;
                    if (saved % 32 == 0)
                    {
                        lock (_store.ProgressLock) progress[key] = cursor;
                        _store.SaveProgress(progress);
                    }
                    await Task.Delay(50, ct); // 单线程 + 让出 CPU
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception ex) { _log.LogError(ex, "预热失败 key={Key} seed={Cursor}", key, cursor); cursor++; }
            lock (_store.ProgressLock) progress[key] = cursor;
            _store.SaveProgress(progress);
            _log.LogInformation("预热切换：key={Key} 下一种子 {Cursor}", key, cursor);
        }
    }
}
