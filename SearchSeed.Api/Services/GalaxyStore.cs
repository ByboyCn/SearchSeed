using System.Collections.Concurrent;
using System.IO.Compression;
using System.Text.Json;
using SearchSeed.Core;

namespace SearchSeed.Api.Services;

// 持久化的"已计算种子"索引：结果落盘 (data/galaxies/*.json.gz) + 游标进度文件
public class GalaxyStore
{
    static readonly JsonSerializerOptions JsonOpts = new() { IncludeFields = true };
    public string Dir { get; }
    readonly string _progressFile;
    readonly ConcurrentDictionary<(int, int, int, bool), byte> _done = new();
    public readonly object ProgressLock = new();

    public GalaxyStore(IWebHostEnvironment env)
    {
        Dir = Path.Combine(env.ContentRootPath, "data", "galaxies");
        Directory.CreateDirectory(Dir);
        _progressFile = Path.Combine(Dir, "_progress.json");
        LoadIndex();
    }

    void LoadIndex()
    {
        foreach (var f in Directory.GetFiles(Dir, "*.json.gz"))
        {
            var stem = Path.GetFileNameWithoutExtension(f); // e.g. s1234_64_4_1
            if (!stem.StartsWith('s')) continue;
            var p = stem[1..].Split('_');
            if (p.Length == 4 && int.TryParse(p[0], out var s) && int.TryParse(p[1], out var n)
                && int.TryParse(p[2], out var r) && int.TryParse(p[3], out var m))
                _done[(s, n, r, m != 0)] = 1;
        }
    }

    public bool IsDone(int seed, int starNum, int resIdx, bool fast) => _done.ContainsKey((seed, starNum, resIdx, fast));

    public void MarkDone(int seed, int starNum, int resIdx, bool fast) => _done[(seed, starNum, resIdx, fast)] = 1;

    static string FileName(int seed, int starNum, int resIdx, bool fast) => $"s{seed}_{starNum}_{resIdx}_{(fast ? 1 : 0)}.json.gz";

    // 剩余空间不足时抛出，调用方（预热）应暂停写盘
    public class DiskFullException : Exception
    {
        public DiskFullException(string msg) : base(msg) { }
    }

    static void CheckDiskSpace(string path)
    {
        var drive = new DriveInfo(Path.GetPathRoot(Path.GetFullPath(path)));
        if (drive.AvailableFreeSpace < 10L * 1024 * 1024 * 1024) // < 10GB
            throw new DiskFullException($"磁盘剩余 {drive.AvailableFreeSpace / 1e9:F1}GB，低于 10GB，暂停写入蓝图缓存");
    }

    public void Save(int seed, int starNum, int resIdx, bool fast, GalaxyResult result)
    {
        CheckDiskSpace(Dir);
        var path = Path.Combine(Dir, FileName(seed, starNum, resIdx, fast));
        var tmp = path + ".tmp";
        using (var fs = File.Create(tmp))
        using (var gz = new GZipStream(fs, CompressionLevel.Fastest))
            JsonSerializer.Serialize(gz, result, JsonOpts);
        File.Move(tmp, path, true);
        MarkDone(seed, starNum, resIdx, fast);
    }

    public GalaxyResult? Load(int seed, int starNum, int resIdx, bool fast)
    {
        var path = Path.Combine(Dir, FileName(seed, starNum, resIdx, fast));
        if (!File.Exists(path)) return null;
        try
        {
            using var fs = File.OpenRead(path);
            using var gz = new GZipStream(fs, CompressionMode.Decompress);
            return JsonSerializer.Deserialize<GalaxyResult>(gz, JsonOpts);
        }
        catch { return null; }
    }

    // 游标：记录每个 (starNum, resIdx, fast) 组合下一个待算的种子
    public Dictionary<string, ulong> LoadProgress()
    {
        try
        {
            if (File.Exists(_progressFile))
                return JsonSerializer.Deserialize<Dictionary<string, ulong>>(File.ReadAllText(_progressFile)) ?? new();
        }
        catch { }
        return new();
    }

    public void SaveProgress(Dictionary<string, ulong> progress)
    {
        lock (ProgressLock)
        {
            var tmp = _progressFile + ".tmp";
            File.WriteAllText(tmp, JsonSerializer.Serialize(progress));
            File.Move(tmp, _progressFile, true);
        }
    }
}