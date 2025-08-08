using System.Net.Http;
using System.Net.Http.Json;

namespace KIOSK.Services;

public enum CemsApiCmd
{
    GetCurrency = 010,
    GetCurrencyList = 011,
}

public interface IApiService
{
    Task<string> SendCommandAsync(string cmd, Dictionary<string, string> parameters);
}

public class CemsApiService : IApiService
{
    private readonly HttpClient _httpClient;
    private const string BaseUrl = "https://cems.moneybox.or.kr/api/cmdV2.php";
    private const string ApiKey = "C4E7I4W5C4B6L3K4T2C4";

    public CemsApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<string> SendCommandAsync(string cmd, Dictionary<string, string> parameters)
    {
        var query = new List<string>
        {
            $"cmd={cmd}",
            $"key={ApiKey}"
        };

        if (parameters != null)
        {
            foreach (var kv in parameters)
            {
                query.Add($"{kv.Key}={Uri.EscapeDataString(kv.Value)}");
            }
        }

        var requestUrl = $"{BaseUrl}?{string.Join("&", query)}";
        var response = await _httpClient.GetAsync(requestUrl);

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }

    public async Task<string> RequestExchangeRateAsync()
    {
        string cmd = "C011";
        
        return await SendCommandAsync(cmd, null);
    }
}