using Devices.Abstractions;
using System.Diagnostics;
using System.Text;

namespace Devices.Devices
{
    public sealed class DeviceQrScanner : IDevice
    {
        private readonly ITransport _transport;
        private readonly SemaphoreSlim _ioGate = new(1, 1); // 직렬화 (프린터 패턴 동일)

        private int _failThreshold = 0;

        public string Name { get; }
        public string Model { get; }

        public DeviceQrScanner(DeviceDescriptor desc, ITransport transport)
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

            try
            {
                if (!_transport.IsOpen)
                    await _transport.OpenAsync(ct);

                byte[] cmd = Encoding.ASCII.GetBytes("~\x01" + "0000" + "#" + "LEDONS*" + ";\x03");

                await _transport.WriteAsync(cmd, ct);

                await Task.Delay(300, ct); // 잠시 대기하여 프린터 응답 준비

                byte[] resp = new byte[256];
                int read = await _transport.ReadAsync(resp, ct);

                if (read > 0)
                {
                    return new DeviceStatusSnapshot
                    (
                        Name: Name,
                        Model: Model,
                        Kind: "NULL",
                        IsPortError: !_transport.IsOpen,
                        IsCommError: false,
                        Timestamp: DateTimeOffset.UtcNow,
                        Alarms: null
                    );
                }
                else
                {
                    _failThreshold++;
                }


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
            //await InitializeAsync(ct);

            await _ioGate.WaitAsync(ct);
            try
            {
                switch (command.Name)
                {
                    case "Scan.Once":
                        return await ScanOnceAsync(ct);

                    case "Scan.Many":
                        return await ScanManyAsync(count: 3, ct); // 고정값 3회

                    case "Scan.TriggerOn":
                        return await SendSoftTriggerAsync(true, ct);

                    case "Scan.TriggerOff":
                        return await SendSoftTriggerAsync(false, ct);

                    case "Scan.Read":
                        {
                            byte[] resp = new byte[256];
                            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
                            timeoutCts.CancelAfter(1000);

                            int read = await _transport.ReadAsync(resp, timeoutCts.Token);

                            return new(true, "");
                        }

                    default:
                        return new(false, $"Unknown command {command.Name}");
                }
            }
            finally { _ioGate.Release(); }
        }

        // === Scan.One 구현 ===
        private async Task<CommandResult> ScanOnceAsync(CancellationToken ct)
        {
            var val = await ReadOneAsync(ct); // 내부에서 5초 관리

            if (val is null)
                return new(false, "Timeout");

            return new(true, "OK", val);
        }

        // === Scan.Many 구현 ===
        private async Task<CommandResult> ScanManyAsync(int count, CancellationToken ct)
        {
            var results = new List<string>();
            for (int i = 0; i < count; i++)
            {
                var val = await ReadOneAsync(ct);

                if (val is null) break;
                results.Add(val);
            }

            return results.Count > 0
                ? new(true, "OK", results)
                : new(false, "No reads");
        }

        private static readonly byte[] TriggerOnCmd =
            { 0x7E, 0x01, 0x30, 0x30, 0x30, 0x30, 0x23, 0x53, 0x43, 0x4E, 0x45, 0x4E, 0x41, 0x31, 0x3B, 0x03 };

        private static readonly byte[] TriggerOffCmd =
            { 0x7E, 0x01, 0x30, 0x30, 0x30, 0x30, 0x23, 0x53, 0x43, 0x4E, 0x45, 0x4E, 0x41, 0x30, 0x3B, 0x03 };

        private async Task<CommandResult> SendSoftTriggerAsync(bool on, CancellationToken ct)
        {
            var cmd = on ? TriggerOnCmd : TriggerOffCmd;

            await _transport.WriteAsync(cmd, ct);

            byte[] resp = new byte[256];

            await Task.Delay(300, ct);

            int read = await _transport.ReadAsync(resp, ct);
            if (read == 0)
                return new(false, "No trigger response");

            // cmd, resp 데이터 출력
            Debug.WriteLine(BitConverter.ToString(cmd));
            Debug.WriteLine(BitConverter.ToString(resp, 0, read));

            return new(true, on ? "Trigger On" : "Trigger Off");
        }

        // === 공통: ASCII 라인 읽기 ===
        private async Task<string?> ReadOneAsync(CancellationToken ct)
        {
            var buf = new List<byte>(128);
            var one = new byte[1];

            // 단일 타이머(누적): ct와 분리해 두는 게 깔끔
            Task timeoutTask = Task.Delay(TimeSpan.FromSeconds(5));

            while (!ct.IsCancellationRequested)
            {
                var readTask = _transport.ReadAsync(one, ct);
                var finished = await Task.WhenAny(readTask, timeoutTask).ConfigureAwait(false);

                if (finished == timeoutTask)
                    return buf.Count == 0 ? null : Encoding.ASCII.GetString(buf.ToArray()).Trim();

                int n = await readTask.ConfigureAwait(false);
                if (n <= 0) // 포트 ReadTimeout(500ms) 등 → 타이머 그대로 유지
                    continue;

                byte b = one[0];
                if (b == (byte)'\r' || b == (byte)'\n')
                {
                    if (buf.Count == 0)
                    {
                        timeoutTask = Task.Delay(TimeSpan.FromSeconds(5));
                        continue;
                    }
                    return Encoding.ASCII.GetString(buf.ToArray()).Trim();
                }

                buf.Add(b);

                // 실제 바이트를 받았을 때만 '비활동 타이머' 리셋
                timeoutTask = Task.Delay(TimeSpan.FromSeconds(5));
            }

            return null;
        }

        public async ValueTask DisposeAsync()
        {
            await _transport.DisposeAsync();
        }
    }
}
