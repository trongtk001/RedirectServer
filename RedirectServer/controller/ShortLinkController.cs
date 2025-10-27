using Microsoft.AspNetCore.Mvc;
using RedirectServer.service;

namespace RedirectServer.controller;

[ApiController]
[Route("api/[controller]")]
public class ShortLinkController(IShortLinkService shortLinkService) : ControllerBase
{
    // GET api/shortlink/{input}
    [HttpGet("{input}")]
    public IActionResult GetLink(string input)
    {
        try
        {
            var link = shortLinkService.GetAdminLink(input);
            return Ok(new { status = 200, url = link });
        }
        catch (Exception ex)
        {
            return BadRequest(new { status = 400, error = ex.Message });
        }
    }

    // GET api/shortlink/go/{input}
    [HttpGet("go/{input}")]
    public IActionResult RedirectToLink(string input)
    {
        try
        {
            var link = shortLinkService.GetAdminLink(input);
            if (string.IsNullOrWhiteSpace(link))
                return BadRequest(new { status = 400, error = "Resolved link is empty" });

            return Redirect(link);
        }
        catch (Exception ex)
        {
            return BadRequest(new { status = 400, error = ex.Message });
        }
    }
    
    // GET api/shortlink/encrypted/{input}
    [HttpGet("go/encrypted/{input}")]
    public async Task<IActionResult> RedirectToEncryptedPacsLink(string input)
    {
        try
        {
            var link = await shortLinkService.GetEncryptedPacsLinkAsync(input);
            
            if (string.IsNullOrWhiteSpace(link))
                return BadRequest(new { status = 400, error = "Resolved link is empty" });
            
            return Redirect(link);
            
        }
        catch (Exception ex)
        {
            return BadRequest(new { status = 400, error = ex.Message });
        }
    }
    
}