using RedirectServer.util;
using System;
using RedirectServer.client;
using RedirectServer.Models;

namespace RedirectServer.service;

public interface IPacsLinkService
{
    PacsLink GetAdminLink(string input);

    Task<PacsLink> GetEncryptedPacsLinkAsync(string input);
}

public class PacsLinkService(IConfiguration configuration, IPacsClient pacsClient) : IPacsLinkService
{
    private static readonly string Key = Environment.GetEnvironmentVariable("key") ?? string.Empty;
    private readonly string _domain = configuration.GetValue<string>("PacsClient:BaseUrl") ?? string.Empty;
    private readonly string _imagePath = configuration.GetValue<string>("PacsClient:ImagePath") ?? string.Empty;

    public PacsLink GetAdminLink(string input)
    {
        var queryParts = BuildQueryParts(input);
        var uri = BuildUri(queryParts, _imagePath);
        return new PacsLink()
        {
            Url = uri,
            Message = "Unencrypted link generated successfully."
        };
    }

    public async Task<PacsLink> GetEncryptedPacsLinkAsync(string input)
    {
        var queryParts = BuildQueryParts(input);
        var token = await pacsClient.GetPacsTokenAsync(string.Join("&", queryParts));
        var uri = BuildUri([$"urltoken={Uri.EscapeDataString(token)}"], _imagePath);

        return new PacsLink()
        {
            Url = uri,
            Token = token,
            Message = "Encrypted link generated successfully."
        };
    }
    
    private static List<string> BuildQueryParts(string input)
    {
        var username = Uri.EscapeDataString(Environment.GetEnvironmentVariable("USERNAME") ?? string.Empty);
        var password = Uri.EscapeDataString(Environment.GetEnvironmentVariable("PASSWORD") ?? string.Empty);
        // var rawParam = DecodeInput(input);
        var encodedPairs = NormalizeAndEncodeQuery(input);

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