using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;

namespace KIOSK.Services;

public interface IApiService
{
    Task<string> SendCommandAsync(string cmd, Dictionary<string, string>? parameters = null, CancellationToken cancellationToken = default);
}

public class CemsApiService : IApiService
{
    private readonly ILoggingService _logging;
    private readonly HttpClient _httpClient;
    private const string BaseUrl = "https://cems.moneybox.or.kr/api/cmdV2.php";
    private readonly string _apiKey;    // apiKey -> ���� �ڵ忡 �ϵ��ڵ� ����

    //IConfiguration config
    public CemsApiService(HttpClient httpClient, ILoggingService logging)
    {
        _logging = logging;
        _httpClient = httpClient;
        _apiKey = "C4E7I4W5C4B6L3K4T2C4";
        // config["Cems:ApiKey"] ?? throw new ArgumentException("Cems:ApiKey missing");
        // �⺻ Ÿ�Ӿƿ��� HttpClient ������ �����ϰų� DI���� ���� ����
        // _httpClient.Timeout = TimeSpan.FromSeconds(10);
    }

    public async Task<string> SendCommandAsync(string cmd, Dictionary<string, string>? parameters = null, CancellationToken cancellationToken = default)
    {
        // ������ ���� ����
        var queryParts = new List<string>
        {
            $"cmd={Uri.EscapeDataString(cmd)}",
            $"key={Uri.EscapeDataString(_apiKey)}"
        };

        if (parameters != null)
        {
            foreach (var kv in parameters)
            {
                queryParts.Add($"{Uri.EscapeDataString(kv.Key)}={Uri.EscapeDataString(kv.Value)}");
            }
        }

        var requestUrl = $"{BaseUrl}?{string.Join("&", queryParts)}";

        // ��õ� (���� �����)
        const int maxAttempts = 3;
        var delay = TimeSpan.FromSeconds(10);

        for (int attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                using var response = await _httpClient.GetAsync(requestUrl, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

                // ������ �ƴ� ��쿡�� ���� ��� ������ status code �˻� �� ó��
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                return content;
            }
            catch (OperationCanceledException ex) when (cancellationToken.IsCancellationRequested)
            {
                _logging.Error(ex, ex.Message);
                // ȣ���ڰ� ����� ��� (����� ���)
                throw new TaskCanceledException("��û�� ȣ���ڿ� ���� ��ҵǾ����ϴ�.", ex);
            }
            catch (TaskCanceledException ex)
            {
                _logging.Error(ex, ex.Message);
                _logging.Info($"Send Retry : {attempt}/{maxAttempts}");
                
                // Ÿ�Ӿƿ����� ���� ��� (HttpClient.Timeout �Ǵ� ���� Ÿ�Ӿƿ�)
                if (attempt == maxAttempts)
                {
                    _logging.Info($"Send Retry Count Over : {attempt}/{maxAttempts}");
                    throw new TimeoutException("��û �ð� �ʰ�(Ÿ�Ӿƿ�) �߻��߽��ϴ�.", ex);
                }

                // ��õ� ���� ���
                await Task.Delay(delay, CancellationToken.None);
                delay = delay * 2;
            }
            catch (HttpRequestException ex)
            {
                _logging.Error(ex, ex.Message);
                _logging.Info($"Send Retry : {attempt}/{maxAttempts}");

                // DNS, ���� ����, SSL �� ��Ʈ��ũ/�������� ����
                if (attempt == maxAttempts)
                {
                    _logging.Info($"Send Retry Count Over : {attempt}/{maxAttempts}");
                    throw new HttpRequestException($"��Ʈ��ũ ��û ����(�õ� {attempt}): {ex.Message}", ex);
                }

                await Task.Delay(delay, CancellationToken.None);
                delay = delay * 2;
            }
            catch (Exception ex)
            {
                _logging.Error(ex, ex.Message);
                throw new Exception("��û ó�� �� �� �� ���� ������ �߻��߽��ϴ�.", ex);
            }
        }

        throw new InvalidOperationException("���� �Ұ� �ڵ�");
    }

    public Task<string> RequestExchangeRateAsync(CancellationToken cancellationToken = default)
    {
        // ���⼱ ���ڿ� cmd�� ���� ���
        string cmd = "C011";
        return SendCommandAsync(cmd, null, cancellationToken);
    }
}
