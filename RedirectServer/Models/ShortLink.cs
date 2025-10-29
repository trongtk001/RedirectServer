using System.ComponentModel.DataAnnotations;

namespace RedirectServer.Models;

public class ShortLink
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(16)]
    public required string ShortCode { get; set; }

    [Required]
    [MaxLength(2000)]
    public required string OriginalUrl { get; set; }
    
    [MaxLength(16)]
    public string? ServiceCode { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int Clicks { get; set; }
}