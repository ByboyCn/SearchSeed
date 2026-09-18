using System.Text.Json;
using SearchSeed.Core;

Console.OutputEncoding = System.Text.Encoding.UTF8;
var svc = new SeedService();
var names = SeedService.VeinNames;
foreach (var f in Directory.GetFiles("tools", "ref_seed*_*.json"))
{
    var refj = JsonDocument.Parse(File.ReadAllText(f)).RootElement;
    int sid = refj.GetProperty("seed").GetInt32();
    int sn = refj.GetProperty("star_num").GetInt32();
    var g = svc.GetGalaxy(sid, sn, 4, fastMode: false);
    int nameMiss = 0;
    foreach (var rs in refj.GetProperty("stars").EnumerateArray())
        if (g.Stars[rs.GetProperty("i").GetInt32()].Name != rs.GetProperty("name").GetString()) nameMiss++;
    long[] mine = new long[14];
    foreach (var st in g.Stars) foreach (var p in st.Planets) foreach (var kv in p.VeinsPoint) mine[Array.IndexOf(names, kv.Key)] += kv.Value;
    var refv = refj.GetProperty("galaxy_veins").EnumerateArray().Select(x => x.GetInt32()).ToArray();
    long diff = 0; for (int i = 0; i < 14; i++) diff += Math.Abs(mine[i] - refv[i]);
    var bh = g.Stars.FirstOrDefault(x => x.Type.Contains("黑洞"));
    Console.WriteLine($"seed={sid} 名字不符={nameMiss} 矿脉diff={diff}({100.0 * diff / refv.Sum():F2}%) 出生星安全度={g.Stars[0].SafetyFactor:P0} 黑洞安全度={bh?.Name}={bh?.SafetyFactor:P0} 等级={bh?.HivePatternLevel}");
}
