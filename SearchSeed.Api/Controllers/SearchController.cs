using Microsoft.AspNetCore.Mvc;
using SearchSeed.Api.Services;

namespace SearchSeed.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SearchController : ControllerBase
{
    readonly SearchService _search;
    public SearchController(SearchService search) => _search = search;

    public class SearchRequest
    {
        public long FromSeed { get; set; } = 0;
        public int StarNumFrom { get; set; } = 64;
        public int StarNumTo { get; set; } = 64;
        public int ResourceIndex { get; set; } = 4;
        public bool FastMode { get; set; } = true;
        public int MaxResults { get; set; } = 100;
        public GalaxyCond Conditions { get; set; } = new();
    }

    [HttpPost("start")]
    public IActionResult Start([FromBody] SearchRequest req)
    {
        var c = req.Conditions;
        if (c.Veins.Point.Count == 0 && c.Veins.Amount.Count == 0 && c.Stars.Count == 0 && c.Planets.Count == 0)
            return BadRequest("至少添加一个条件（星区矿脉 / 恒星系条件 / 行星条件）");
        var job = _search.Start(c, req.FromSeed, req.StarNumFrom, req.StarNumTo, req.ResourceIndex, req.FastMode, req.MaxResults);
        return Ok(new { jobId = job.Id });
    }

    [HttpGet("{jobId}")]
    public IActionResult Status(string jobId)
    {
        if (!_search.Jobs.TryGetValue(jobId, out var job)) return NotFound();
        return Ok(new
        {
            jobId,
            finished = job.Finished,
            stopped = job.Stopped,
            scanned = job.Scanned,
            skipped = job.Skipped,
            matches = job.Matches.ToList(),
            maxResults = job.MaxResults,
        });
    }

    [HttpPost("{jobId}/stop")]
    public IActionResult Stop(string jobId)
    {
        if (!_search.Jobs.TryGetValue(jobId, out var job)) return NotFound();
        job.Stopped = true;
        return Ok();
    }
}