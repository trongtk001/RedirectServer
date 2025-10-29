namespace RedirectServer.Models;

public class PacsLink
{
    public required string Url { get; set; }
    public string? Token { get; set; }
    public string? Message { get; set; }
}