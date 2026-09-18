using SearchSeed.Core;

Console.OutputEncoding = System.Text.Encoding.UTF8;
var svc = new SeedService();
var names = SeedService.VeinNames;
foreach (var f in Directory.GetFiles("tools", "ref_seed*_*.json"))
{
    var refj = System.Text.Json.JsonDocument.Parse(File.ReadAllText(f)).RootElement;
    int sid = refj.GetProperty("seed").GetInt32();
    int sn = refj.GetProperty("star_num").GetInt32();
    var sw = System.Diagnostics.Stopwatch.StartNew();
    var g = svc.GetGalaxy(sid, sn, 4, fastMode: false);
    sw.Stop();
    long[] mine = new long[14];
    foreach (var s in g.Stars) foreach (var p in s.Planets) foreach (var kv in p.VeinsPoint) mine[Array.IndexOf(names, kv.Key)] += kv.Value;
    var refv = refj.GetProperty("galaxy_veins").EnumerateArray().Select(x => x.GetInt32()).ToArray();
    long diff = 0; for (int i = 0; i < 14; i++) diff += Math.Abs(mine[i] - refv[i]);
    long total = refv.Sum();
    Console.WriteLine($"seed={sid} stars={sn} {sw.ElapsedMilliseconds}ms ref={total} mine={mine.Sum()} diff={diff} ({100.0 * diff / total:F3}%)");
}
