using System.ComponentModel.DataAnnotations;

namespace RedirectServer.Request;

public class CreateRequest
{
    [Required] public required string OriginalUrl { get; set; }
    public string? ServiceCode { get; set; }
}