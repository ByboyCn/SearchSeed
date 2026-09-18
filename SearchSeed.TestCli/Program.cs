using System.Text.Json;
using SearchSeed.Core;
Console.OutputEncoding = System.Text.Encoding.UTF8;
var svc = new SeedService();
var g = svc.GetGalaxy(1234, 64, 4, fastMode: false);
var p = g.Stars[0].Planets.First(x => !x.IsSatellite);
var j = JsonSerializer.Serialize(new { p.Name, p.Liquid, p.LandPercent, p.OrbitRadius, p.OrbitalPeriodSec, p.RotationPeriodSec, p.OrbitInclination, p.OrbitLongitude, p.Obliquity }, new JsonSerializerOptions { IncludeFields = true });
Console.WriteLine(j);
