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
    private readonly string _apiKey;    // apiKey -> 절대 코드에 하드코딩 금지

    //IConfiguration config
    public CemsApiService(HttpClient httpClient, ILoggingService logging)
    {
        _logging = logging;
        _httpClient = httpClient;
        _apiKey = "C4E7I4W5C4B6L3K4T2C4";
        // config["Cems:ApiKey"] ?? throw new ArgumentException("Cems:ApiKey missing");
        // 기본 타임아웃은 HttpClient 생성시 설정하거나 DI에서 구성 권장
        // _httpClient.Timeout = TimeSpan.FromSeconds(10);
    }

    public async Task<string> SendCommandAsync(string cmd, Dictionary<string, string>? parameters = null, CancellationToken cancellationToken = default)
    {
        // 안전한 쿼리 빌드
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

        // 재시도 (지수 백오프)
        const int maxAttempts = 3;
        var delay = TimeSpan.FromSeconds(10);

        for (int attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                using var response = await _httpClient.GetAsync(requestUrl, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

                // 성공이 아닌 경우에도 내용 얻고 싶으면 status code 검사 후 처리
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                return content;
            }
            catch (OperationCanceledException ex) when (cancellationToken.IsCancellationRequested)
            {
                _logging.Error(ex, ex.Message);
                // 호출자가 취소한 경우 (명시적 취소)
                throw new TaskCanceledException("요청이 호출자에 의해 취소되었습니다.", ex);
            }
            catch (TaskCanceledException ex)
            {
                _logging.Error(ex, ex.Message);
                _logging.Info($"Send Retry : {attempt}/{maxAttempts}");
                
                // 타임아웃으로 인한 취소 (HttpClient.Timeout 또는 내부 타임아웃)
                if (attempt == maxAttempts)
                {
                    _logging.Info($"Send Retry Count Over : {attempt}/{maxAttempts}");
                    throw new TimeoutException("요청 시간 초과(타임아웃) 발생했습니다.", ex);
                }

                // 재시도 전에 대기
                await Task.Delay(delay, CancellationToken.None);
                delay = delay * 2;
            }
            catch (HttpRequestException ex)
            {
                _logging.Error(ex, ex.Message);
                _logging.Info($"Send Retry : {attempt}/{maxAttempts}");

                // DNS, 연결 실패, SSL 등 네트워크/프로토콜 오류
                if (attempt == maxAttempts)
                {
                    _logging.Info($"Send Retry Count Over : {attempt}/{maxAttempts}");
                    throw new HttpRequestException($"네트워크 요청 실패(시도 {attempt}): {ex.Message}", ex);
                }

                await Task.Delay(delay, CancellationToken.None);
                delay = delay * 2;
            }
            catch (Exception ex)
            {
                _logging.Error(ex, ex.Message);
                throw new Exception("요청 처리 중 알 수 없는 오류가 발생했습니다.", ex);
            }
        }

        throw new InvalidOperationException("도달 불가 코드");
    }

    public Task<string> RequestExchangeRateAsync(CancellationToken cancellationToken = default)
    {
        // 여기선 문자열 cmd를 직접 사용
        string cmd = "C011";
        return SendCommandAsync(cmd, null, cancellationToken);
    }
}
