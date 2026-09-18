using Microsoft.AspNetCore.Mvc;
using SearchSeed.Core;

namespace SearchSeed.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SeedController : ControllerBase
{
    static readonly SeedService Service = new();

    /// <summary>获取一个种子的完整星系数据（快速模式矿脉）</summary>
    [HttpGet("{seedId}")]
    public GalaxyResult Get(int seedId, int starNum = 64, int resourceIndex = 4, string mode = "standard")
        => Service.GetGalaxy(seedId, starNum, resourceIndex, fastMode: mode == "fast");
}
