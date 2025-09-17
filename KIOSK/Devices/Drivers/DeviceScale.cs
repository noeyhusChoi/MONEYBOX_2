// Devices/ScaleDevice.cs
using Device.Abstractions;
using System;
using System.Data;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Device.Devices
{
    /// <summary>
    /// 예시: ASCII 라인 기반 저울
    ///   - 상태: "READY" / "BUSY" / "ERROR"
    ///   - 명령: READ (현재 무게 요청) -> "W:001.234kg"
    /// </summary>
    public sealed class DeviceScale : IDevice
    {
        private readonly ITransport _transport;
        private readonly IProtocol _protocol;

        public string Name { get; }
        public string Model { get; }

        public DeviceScale(DeviceDescriptor desc, ITransport transport, IProtocol protocol)
        {
            Name = desc.Name;
            Model = desc.Model;
            _transport = transport;
            _protocol = protocol;
        }

        public async Task InitializeAsync(CancellationToken ct = default)
        {
            if (!_transport.IsOpen) await _transport.OpenAsync(ct);
            // 필요 시 초기화 시퀀스 수행
        }

        public async Task<DeviceStatusSnapshot> GetStatusAsync(CancellationToken ct = default, string temp = "임시메소드")
        {
            return new DeviceStatusSnapshot
              (
                  Name: "NULL",
                  Model: "NULL",
                  Kind: "NULL",
                  IsPortError: !_transport.IsOpen,
                  IsCommError: false,
                  Timestamp: DateTimeOffset.UtcNow,
                  Alarms: null
              );
        }

        public async Task<CommandResult> ExecuteAsync(DeviceCommand command, CancellationToken ct = default)
        {
            switch (command.Name)
            {
                case "ReadWeight":
                    {
                        var req = Encoding.ASCII.GetBytes("READ");
                        var resp = await _protocol.ExchangeAsync(_transport, req, 800, ct);
                        var s = Encoding.ASCII.GetString(resp); // ex) "W:001.234kg"
                        return new CommandResult(true, "OK", s);
                    }
                default:
                    return new CommandResult(false, $"Unknown command: {command.Name}");
            }
        }
    }
}
