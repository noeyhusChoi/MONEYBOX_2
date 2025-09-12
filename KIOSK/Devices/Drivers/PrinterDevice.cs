// Devices/PrinterDevice.cs
using Device.Abstractions;
using System.Text;

namespace Device.Devices
{
    public sealed class PrinterDevice : IDevice
    {
        private readonly ITransport _transport;
        public string Name { get; }
        public string Model { get; }

        private int _failThreshold = 0;

        public PrinterDevice(DeviceDescriptor desc, ITransport transport)
        {
            Name = desc.Name;
            Model = desc.Model;
            _transport = transport;
        }

        public async Task InitializeAsync(CancellationToken ct = default)
        {
            try
            {
                if (_transport.IsOpen)
                    await _transport.CloseAsync(ct);

                if (!_transport.IsOpen)
                    await _transport.OpenAsync(ct);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to open transport", ex);
            }
        }

        public async Task<DeviceStatusSnapshot> GetStatusAsync(CancellationToken ct = default, string temp = "임시메소드")
        {
            var alarms = new List<DeviceAlarm>();

            try
            {
                if (!_transport.IsOpen)
                    await _transport.OpenAsync(ct);

                byte[] cmd = { 0x1D, 0x72, 0x01 };
                await _transport.WriteAsync(cmd, ct);

                await Task.Delay(300, ct); // 잠시 대기하여 프린터 응답 준비

                byte[] resp = new byte[8];
                int read = await _transport.ReadAsync(resp, ct);

                // 상태 명령 콜백 처리
                if (read > 0)
                {
                    _failThreshold = 0;

                    if ((resp[0] & 0x01) != 0) { alarms.Add(new DeviceAlarm("PRINT", "용지 없음", Severity.Warning, DateTime.UtcNow)); }
                    if ((resp[0] & 0x02) != 0) { alarms.Add(new DeviceAlarm("PRINT", "헤드 업", Severity.Warning, DateTime.UtcNow)); }
                    if ((resp[0] & 0x04) != 0) { alarms.Add(new DeviceAlarm("PRINT", "용지 에러 있음", Severity.Warning, DateTime.UtcNow)); }
                    if ((resp[0] & 0x08) != 0) { alarms.Add(new DeviceAlarm("PRINT", "용지 잔량 적음", Severity.Warning, DateTime.UtcNow)); }
                    if ((resp[0] & 0x10) != 0) { alarms.Add(new DeviceAlarm("PRINT", "프린트 진행중", Severity.Info, DateTime.UtcNow)); }
                    if ((resp[0] & 0x20) != 0) { alarms.Add(new DeviceAlarm("PRINT", "커터 에러 있음", Severity.Warning, DateTime.UtcNow)); }
                    if ((resp[0] & 0x80) != 0) { alarms.Add(new DeviceAlarm("PRINT", "보조 센서 용지 있음", Severity.Warning, DateTime.UtcNow)); }

                    return new DeviceStatusSnapshot
                    (
                        Name: Name,
                        Model: Model,
                        Kind: "PRINTER",
                        IsPortError: !_transport.IsOpen,
                        IsCommError: false,
                        Timestamp: DateTimeOffset.UtcNow,
                        Alarms: alarms
                    );
                }
                // 상태 명령 콜백 실패
                else
                {
                    _failThreshold++;
                }
            }
            catch (OperationCanceledException)
            {
                return null;
            }
            catch (Exception ex)
            {
                _failThreshold++;
            }

            // 콜백 실패 일정 횟수 이상 시 통신 에러 처리
            if (_failThreshold > 0)
            {
                return new DeviceStatusSnapshot
                (
                    Name: Name,
                    Model: Model,
                    Kind: "PRINTER",
                    IsPortError: !_transport.IsOpen,
                    IsCommError: true,
                    Timestamp: DateTimeOffset.UtcNow,
                    Alarms: new List<DeviceAlarm> { new DeviceAlarm("PRINT", "응답 없음", Severity.Error, DateTime.UtcNow) }
                );
            }

            return null;
        }

        public async Task<CommandResult> ExecuteAsync(DeviceCommand command, CancellationToken ct = default)
        {
            if (command.Name == "PrintText" && command.Payload is string text)
            {
                try
                {
                    byte[] payload = Encoding.GetEncoding("ks_c_5601-1987").GetBytes(text);
                    await _transport.WriteAsync(payload, ct);
                    return new CommandResult(true, "Printed");
                }
                catch
                {

                }
            }

            if (command.Name == "Cut")
            {
                try
                {
                    byte[] payload = new byte[] { 0x1B, 0x69 };
                    await _transport.WriteAsync(payload, ct);
                    return new CommandResult(true, "Printed");
                }
                catch
                {
                    return new CommandResult(false, "Print failed");
                }
            }

            if (command.Name == "QR" && command.Payload is string data)
            {
                int maxLength = 230;
                byte[] buf = Encoding.GetEncoding("ks_c_5601-1987").GetBytes(data);

                if (buf.Length > maxLength)
                    return new CommandResult(false, "Data length exceeded");

                byte dataLength = (byte)(buf.Length & 0xFF);
                byte type = buf.Length switch
                {
                    <= 18 => 1,
                    <= 54 => 3,
                    <= 106 => 5,
                    <= 230 => 9,
                    _ => 9
                };

                byte[] cmd = { 0x1A, 0x42, 0x02, dataLength, type };

                byte[] packet = new byte[cmd.Length + buf.Length];
                Buffer.BlockCopy(cmd, 0, packet, 0, cmd.Length);
                Buffer.BlockCopy(buf, 0, packet, cmd.Length, buf.Length);

                await _transport.WriteAsync(packet, ct);
                return new CommandResult(true, "QR Printed");
            }

            return new CommandResult(false, "Unknown Command");
        }
    }
}
