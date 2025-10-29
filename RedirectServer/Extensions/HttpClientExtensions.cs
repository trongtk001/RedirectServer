using RedirectServer.client;

namespace RedirectServer.Extensions;

public static class HttpClientExtensions
{
    public static void AddPacsHttpClient(this IServiceCollection services, IConfiguration configuration)
    {
        var baseAddress = "https://mockfast.io";// configuration.GetValue<string>("PacsClient:BaseUrl") ?? string.Empty;
        
        if (string.IsNullOrEmpty(baseAddress))
            throw new Exception("PacsClient BaseAddress is not configured.");
        
        services.AddHttpClient<IPacsClient, PacsClient>(client =>
        {
            client.BaseAddress = new Uri(baseAddress);
            client.Timeout = TimeSpan.FromSeconds(10);
        });
    }
}