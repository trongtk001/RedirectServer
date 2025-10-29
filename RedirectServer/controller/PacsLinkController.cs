using Microsoft.AspNetCore.Mvc;
using RedirectServer.service;

namespace RedirectServer.controller;

[ApiController]
[Route("api/[controller]")]
public class PacsLinkController : ControllerBase
{
    private readonly IPacsLinkService _pacsLinkService;

    public PacsLinkController(IPacsLinkService pacsLinkService)
    {
        _pacsLinkService = pacsLinkService ?? throw new ArgumentNullException(nameof(pacsLinkService));
    }

    [HttpGet("unencrypted")]
    public IActionResult GetAdmin([FromQuery] string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return BadRequest(new { error = "input query is required" });

        var link = _pacsLinkService.GetAdminLink(input);
        return Ok(link);
    }

    [HttpGet("encrypted")]
    public async Task<IActionResult> GetEncrypted([FromQuery] string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return BadRequest(new { error = "input query is required" });

        try
        {
            var link = await _pacsLinkService.GetEncryptedPacsLinkAsync(input);
            return Ok(link);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }
}