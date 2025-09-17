using Device.Abstractions;
using Newtonsoft.Json;
using Pr22;
using Pr22.Events;
using Pr22.Imaging;
using Pr22.Processing;
using Pr22.Task;
using System.Diagnostics;
using System.IO;
using WpfApp1.NewFolder;
using Path = System.IO.Path;

namespace Device.Devices
{
    public sealed class DeviceIdScanner : IDevice
    {
        private readonly ITransport _transport;
        private readonly SemaphoreSlim _ioGate = new(1, 1);

        private int _failThreshold; // 통신 실패 카운트(스냅샷용)

        public string Name { get; }
        public string Model { get; }

        // PR22 전용
        private DocumentReaderDevice _dev;
        public event EventHandler<(int page, Light light, string path)>? ImageSaved;
        public event EventHandler<ScanEvent>? ScanSequence;

        // 
        private Pr22.Util.PresenceState _presenceState = Pr22.Util.PresenceState.Empty;
        private Page _page = null;

        // 이벤트 중복 등록 방지
        private readonly object _presenceLock = new object();
        private bool _presenceSubscribed = false;

        public enum ScanEvent
        {
            Empty,
            Scanning,
            ScanComplete,
            Removed,
            RemovalTimeout
        }

        public DeviceIdScanner(DeviceDescriptor desc, ITransport transport)
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
                    case "ScanStart":
                        {
                            var res = await ScanStart();
                            return new CommandResult(res, "");
                        }
                    case "ScanStop":
                        {
                            var res = await ScanStop();
                            return new CommandResult(res, "");
                        }
                    case "GetScanStatus":
                        {
                            // 구독 중 아닐 때, 구독 시작
                            if (!_presenceSubscribed)
                            {
                                await ScanStart();
                                return new CommandResult(false, "", null);
                            }
                            else
                            {
                                return new CommandResult(true, "", _presenceState);
                            }
                        }
                    case "SaveImage":
                        {
                            var res = await SaveImage();
                            return new CommandResult(res, "");
                        }

                    case "ProcessOCR":
                        {
                            var res = await ProcessOCR();
                            return new CommandResult(res, "");
                        }
                    default:
                        return new(false, $"Unknown command {command.Name}");
                }
            }
            finally { _ioGate.Release(); }
        }

        private void OnPresence(object? s, DetectionEventArgs e)
        {
            try
            {
                switch (e.State)
                {
                    case Pr22.Util.PresenceState.Empty:
                        _presenceState = Pr22.Util.PresenceState.Empty;
                        break;
                    case Pr22.Util.PresenceState.Moving:
                        _presenceState = Pr22.Util.PresenceState.Moving;
                        break;
                    case Pr22.Util.PresenceState.Present:
                    case Pr22.Util.PresenceState.NoMove:
                        _presenceState = Pr22.Util.PresenceState.NoMove;
                        break;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("OnPresence 처리 예외: " + ex);
            }
        }

        #region Utils
        private async Task<bool> ScanStart()
        {
            try
            {
                lock (_presenceLock)
                {
                    if (!_presenceSubscribed)
                    {
                        _dev.PresenceStateChanged += OnPresence;
                        _presenceSubscribed = true;
                    }
                }

                _dev.Scanner.StartTask(FreerunTask.Detection());

                return true;
            }
            catch
            {
                return false;
            }
        }

        private async Task<bool> ScanStop()
        {
            try
            {
                lock (_presenceLock)
                {
                    if (_presenceSubscribed)
                    {
                        _dev.PresenceStateChanged -= OnPresence;
                        _presenceSubscribed = false;
                    }
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        private async Task<bool> SaveImage()
        {
            try
            {
                var task = new DocScannerTask() { };
                task.Add(Light.White).Add(Light.Infra);
                Pr22.Processing.Page page = _dev.Scanner.Scan(task, PagePosition.First);

                // 저장 경로
                var saveDir = Path.Combine(Environment.CurrentDirectory, "ScanOutput");
                Directory.CreateDirectory(saveDir);

                _page = page;

                try
                {
                    var img = page.Select(Light.White).GetImage();
                    var fileName = $"scan_{Light.White}.jpg";
                    var path = Path.Combine(saveDir, fileName);
                    img.Save(RawImage.FileFormat.Jpeg).Save(path);

                    img = page.Select(Light.Infra).GetImage();
                    fileName = $"scan_{Light.Infra}.jpg";
                    path = Path.Combine(saveDir, fileName);
                    img.Save(RawImage.FileFormat.Jpeg).Save(path);

                    return true;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"이미지 저장 실패 {ex.Message}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Scan/Save 처리중 예외: " + ex);
                return false;
            }
        }

        private async Task<bool> ProcessOCR()
        {
            return false;
        }
        #endregion

        public class JsonData
        {
            public string type { get; set; }
            public string id { get; set; }
            public string id_confidence { get; set; }
            public string name { get; set; }
            public string name_confidence { get; set; }
            public string address { get; set; }
            public string address_confidence { get; set; }
            public string nation { get; set; }
            public string nation_confidence { get; set; }
            public string comment { get; set; }
            public bool rotate_image { get; set; }
            public bool need_save_original { get; set; }
        }
    }
}
