using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using SearchSeed.Api.Services;

namespace SearchSeed.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BlueprintController : ControllerBase
{
    readonly BlueprintService _bp;
    public BlueprintController(BlueprintService bp) => _bp = bp;

    // 支持 multipart 文件上传 或 直接 body（文本/原始字节）
    [HttpPost]
    [RequestSizeLimit(20_000_000)]
    public async Task<IActionResult> Parse()
    {
        try
        {
            byte[] data;
            var ct = Request.ContentType ?? "";
            if (ct.StartsWith("multipart/", StringComparison.OrdinalIgnoreCase) && Request.Form.Files.Count > 0)
            {
                using var ms = new MemoryStream();
                await Request.Form.Files[0].CopyToAsync(ms);
                data = ms.ToArray();
            }
            else
            {
                using var ms = new MemoryStream();
                await Request.Body.CopyToAsync(ms);
                data = ms.ToArray();
            }
            if (data.Length == 0) return BadRequest("空的蓝图数据");
            return Ok(_bp.Parse(data));
        }
        catch (ArgumentException ex) { return BadRequest(ex.Message); }
        catch (InvalidDataException ex) { return BadRequest("蓝图数据无效: " + ex.Message); }
        catch (JsonException) { return BadRequest("JSON 解析失败：蓝图数据损坏或版本不支持"); }
        catch (FormatException) { return BadRequest("base64 解析失败"); }
        catch (Exception ex) { return StatusCode(500, "解析失败: " + ex.Message); }
    }
}