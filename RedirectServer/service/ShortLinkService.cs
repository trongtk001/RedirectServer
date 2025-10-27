using RedirectServer.util;
using System;
using RedirectServer.client;

namespace RedirectServer.service;

public interface IShortLinkService
{
    string GetAdminLink(string input);

    Task<string> GetEncryptedPacsLinkAsync(string input);
}

public class ShortLinkService : IShortLinkService
{
    
    private static readonly string Key = Environment.GetEnvironmentVariable("key") ?? string.Empty;
    private readonly string _domain;
    private readonly IPacsClient _pacsClient;

    public ShortLinkService(IConfiguration configuration, IPacsClient pacsClient)
    {
        _pacsClient = pacsClient;
        var section = configuration.GetSection("PacsClient");
        _domain = section.GetValue<string>("BaseUrl") ?? string.Empty;
    }

    public string GetAdminLink(string input)
    {
        var queryParts = BuildQueryParts(input, "admin");
        return BuildUri(queryParts, "portal");
    }

    public async Task<string> GetEncryptedPacsLinkAsync(string input)
    {
        var queryParts = BuildQueryParts(input, "encrypted");
        var token = await _pacsClient.GetPacsTokenAsync(string.Join("&", queryParts));
        return BuildUri([$"urltoken={Uri.EscapeDataString(token)}"], "portal");
    }
    
    private static List<string> BuildQueryParts(string input, string path)
    {
        var username = Uri.EscapeDataString(Environment.GetEnvironmentVariable("USERNAME") ?? string.Empty);
        var password = Uri.EscapeDataString(Environment.GetEnvironmentVariable("PASSWORD") ?? string.Empty);
        var rawParam = DecodeInput(input);
        var encodedPairs = NormalizeAndEncodeQuery(rawParam);

        var queryParts = new List<string>
        {
            $"username={username}",
            $"password={password}",
            "hide_top=all",
            "hide_sides=history"
        };

        queryParts.AddRange(encodedPairs);
        return queryParts;
    }
    
    private string BuildUri(List<string> queryParts, string path)
    {
        var baseUri = new Uri(_domain);
        return new UriBuilder(baseUri)
        {
            Path = $"{baseUri.AbsolutePath}{path}",
            Query = string.Join("&", queryParts)
        }.Uri.ToString();
    }

    private static string DecodeInput(string input)
    {
        if (string.IsNullOrEmpty(Key))
            throw new Exception("No key provided");

        return DecodeXorBase64.Decode(input, Key);
    }

    private static List<string> NormalizeAndEncodeQuery(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return new List<string>();

        var parts = raw.Split('&', StringSplitOptions.RemoveEmptyEntries);
        var list = new List<string>(parts.Length);

        foreach (var part in parts)
        {
            var idx = part.IndexOf('=');
            if (idx <= 0) throw new FormatException($"Invalid query part: {part}");

            var key = Uri.EscapeDataString(part[..idx].Trim());
            var value = Uri.EscapeDataString(part[(idx + 1)..].Trim());
            list.Add($"{key}={value}");
        }

        return list;
    }
}