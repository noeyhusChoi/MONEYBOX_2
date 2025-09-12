using Device.Abstractions;
using Pr22;
using System.Diagnostics;

namespace Device.Devices
{
    public sealed class IdScannerDevice : IDevice
    {
        private readonly ITransport _transport;
        private readonly SemaphoreSlim _ioGate = new(1, 1);

        private int _failThreshold; // 통신 실패 카운트(스냅샷용)

        public string Name { get; }
        public string Model { get; }

        private DocumentReaderDevice _dev;

        public IdScannerDevice(DeviceDescriptor desc, ITransport transport)
        {
            Name = desc.Name;
            Model = desc.Model;
            _transport = transport;
        }

        public async Task InitializeAsync(CancellationToken ct = default)
        {
            try
            {
                if (_dev != null)
                {
                    _dev.Close();
                    _dev.Dispose();
                }

                var dev = new DocumentReaderDevice();

                var list = DocumentReaderDevice.GetDeviceList();
                if (list.Count == 0)
                    throw new Pr22.Exceptions.NoSuchDevice("No device found.");

                dev.UseDevice(list[0]);
                _dev = dev;

                Debug.WriteLine("Device connected: " + _dev.DeviceName);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to open transport", ex);
            }
        }

        public async Task<DeviceStatusSnapshot> GetStatusAsync(CancellationToken ct = default, string temp = "")
        {
            await _ioGate.WaitAsync(ct);
            try
            {
                var info = _dev.Scanner.Info;

                info.IsCalibrated();    // 장치 연결 상태 체크

                return new DeviceStatusSnapshot
                (
                    Name: Name,
                    Model: Model,
                    Kind: "NULL",
                    IsPortError: false,
                    IsCommError: false,
                    Timestamp: DateTimeOffset.UtcNow,
                    Alarms: null
                );
            }
            catch
            {
                return new DeviceStatusSnapshot
                (
                    Name: Name,
                    Model: Model,
                    Kind: "NULL",
                    IsPortError: true,
                    IsCommError: true,
                    Timestamp: DateTimeOffset.UtcNow,
                    Alarms: null
                );
            }
            finally { _ioGate.Release(); }
        }

        public async Task<CommandResult> ExecuteAsync(DeviceCommand command, CancellationToken ct = default)
        {
            //await InitializeAsync(ct);

            await _ioGate.WaitAsync(ct);
            try
            {
                // TODO : 명령어 처리 로직 구현
                switch (command.Name)
                {
                    case "ID.TODO":
                        return new(false, $"Unknown command {command.Name}");

                    default:
                        return new(false, $"Unknown command {command.Name}");
                }
            }
            finally { _ioGate.Release(); }
        }
    }
}
