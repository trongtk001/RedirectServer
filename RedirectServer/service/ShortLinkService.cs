using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using RedirectServer.Data;
using RedirectServer.Models;
using RedirectServer.Request;

namespace RedirectServer.service;

public interface IShortLinkService
{
    Task<ShortLink> CreateAsync(CreateRequest createRequest, int length = 7);

    Task<ShortLink?> ResolveAsync(string code);
}

public class ShortLinkService : IShortLinkService
{
    private const string Base62Chars = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private readonly RandomNumberGenerator _rng = RandomNumberGenerator.Create();
    private readonly AppDbContext _db;

    public ShortLinkService(AppDbContext db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public async Task<ShortLink> CreateAsync(CreateRequest createRequest, int length = 7)
    {
        if (createRequest == null) throw new ArgumentNullException(nameof(createRequest));
        var originalUrl = createRequest.OriginalUrl;
        if (string.IsNullOrWhiteSpace(originalUrl)) throw new ArgumentException(nameof(createRequest.OriginalUrl));
        originalUrl = originalUrl.Trim();

        // Try to find existing mapping
        // var existing = await _db.ShortLinks.FirstOrDefaultAsync(s => s.OriginalUrl == originalUrl);
        // if (existing != null) return existing;

        // Generate a unique short code
        for (int attempt = 0; attempt < 10; attempt++)
        {
            var code = GenerateShortCode(length);
            if (!await _db.ShortLinks.AnyAsync(s => s.ShortCode == code))
            {
                var entry = new ShortLink { ShortCode = code, OriginalUrl = originalUrl, ServiceCode = createRequest.ServiceCode };
                _db.ShortLinks.Add(entry);
                await _db.SaveChangesAsync();
                return entry;
            }
        }

        // fallback using guid-based code
        var fallbackCode = Guid.NewGuid().ToString("n").Substring(0, length);
        var fallbackEntry = new ShortLink { ShortCode = fallbackCode, OriginalUrl = originalUrl };
        _db.ShortLinks.Add(fallbackEntry);
        await _db.SaveChangesAsync();
        return fallbackEntry;
    }

    public async Task<ShortLink?> ResolveAsync(string code)
    {
        if (string.IsNullOrWhiteSpace(code)) return null;
        var entry = await _db.ShortLinks.FirstOrDefaultAsync(s => s.ShortCode == code);
        if (entry == null) return entry;
        entry.Clicks++;
        await _db.SaveChangesAsync();

        return entry;
    }

    private string GenerateShortCode(int length)
    {
        var bytes = new byte[length];
        _rng.GetBytes(bytes);
        var sb = new StringBuilder(length);
        foreach (var b in bytes)
        {
            sb.Append(Base62Chars[b % Base62Chars.Length]);
        }

        return sb.ToString();
    }
}