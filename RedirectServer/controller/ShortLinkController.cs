using Microsoft.AspNetCore.Mvc;
using RedirectServer.Request;
using RedirectServer.service;

namespace RedirectServer.controller;

[ApiController]
public class ShortLinkController(IShortLinkService shortLinkService) : ControllerBase
{
    private readonly IShortLinkService _shortLinkService = shortLinkService ?? throw new ArgumentNullException(nameof(shortLinkService));

    [HttpPost("shortlinks")]
    public async Task<IActionResult> Create([FromBody] CreateRequest req)
    {
        // Validate OriginalUrl; parameter is non-nullable so explicit null check is redundant
        if (string.IsNullOrWhiteSpace(req.OriginalUrl))
            return BadRequest(new { error = "OriginalUrl is required" });

        var original = req.OriginalUrl.Trim();
        if (!Uri.TryCreate(original, UriKind.Absolute, out var _))
            return BadRequest(new { error = "Invalid URL" });

        var entry = await _shortLinkService.CreateAsync(req);
        var scheme = Request.Scheme;
        var host = Request.Host.Value;
        var shortUrl = $"{scheme}://{host}/{entry.ShortCode}";
        return Ok(new { shortUrl, code = entry.ShortCode });
    }

    [HttpGet("shortlinks/{code}")]
    public async Task<IActionResult> Info(string code)
    {
        var entry = await _shortLinkService
            .ResolveAsync(code); // increments clicks; if you don't want that, separate method
        if (entry == null) return NotFound();
        return Ok(new
        {
            code = entry.ShortCode,
            originalUrl = entry.OriginalUrl,
            createdAt = entry.CreatedAt,
            clicks = entry.Clicks
        });
    }

    [HttpGet("{code}")]
    public async Task<IActionResult> RedirectToOriginal(string code)
    {
        var entry = await _shortLinkService.ResolveAsync(code);
        if (entry == null) return NotFound();
        return RedirectPreserveMethod(entry.OriginalUrl);
    }
}