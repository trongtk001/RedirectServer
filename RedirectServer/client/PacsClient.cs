using System.Net.Mime;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RedirectServer.client;

public interface IPacsClient
{
     Task<string> GetPacsTokenAsync(string queryString);
}

public class PacsClient(HttpClient httpClient, IConfiguration configuration) : IPacsClient
{
    public async Task<string> GetPacsTokenAsync(string queryString)
    {
        var requestUrl = "backend/apitemplate/post/FOU7Q96SWB"; // configuration.GetValue<string>("PacsClient:EncryptPath");

        var body = new
        {
            QueryString = queryString
        };

        var json = JsonConvert.SerializeObject(body);
        var content = new StringContent(json, Encoding.UTF8, MediaTypeNames.Application.Json);

        var response = await httpClient.PostAsync(requestUrl, content);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"PACS API Error: {response.StatusCode} - {error}");
        }

        var responseJson = await response.Content.ReadAsStringAsync();
        var obj = JObject.Parse(responseJson);
        var token = obj["token"]?.ToString();

        if (string.IsNullOrEmpty(token))
            throw new InvalidOperationException("Response does not contain a token.");

        return token;
    }
}