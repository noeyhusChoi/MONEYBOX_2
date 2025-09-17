// Abstractions/Models.cs
using System.Data;

namespace Devices.Abstractions
{
    public record DeviceCommand(string Name, object? Payload = null);
    public record CommandResult(bool Success, string Message = "", object? Data = null);

    //public record DeviceDescriptor(
    //    string Id,
    //    string Model,
    //    string TransportName,   // ex) "COM3@115200"
    //    string ProtocolName,    // ex) "SimpleCRC"
    //    int PollingMs = 1000
    //);

    // 공통 열거형
    public enum DeviceHealth { Ok, Degraded, Error, Unknown }
    public enum Severity { Info, Warning, Error, Critical }

    // 알람/에러 항목(선택)
    public sealed record DeviceAlarm(
        string Code,               // 예: "PAPER.OUT", "CUTTER.ERROR"
        string Message,            // 사용자 메시지
        Severity Severity,         // 중요도
        DateTimeOffset At
    );

    // 단일 스냅샷(엔벨로프 + 유연 페이로드)
    public sealed record DeviceStatusSnapshot(
        string Name,                // 장치 식별자 (desc.Name)
        string Model,               // 모델 (desc.Model)
        string Kind,                // 논리 종류 (예: "Printer","QR","Scale"...)
        bool IsPortError,           // 포트 오류 여부
        bool IsCommError,           // 통신 오류 여부
        DateTimeOffset Timestamp,   // 상태 시각(UTC 권장)

        // ---- 유연 페이로드 ----
        IReadOnlyCollection<DeviceAlarm>? Alarms         // 알람/에러 목록(선택)
    );
}
