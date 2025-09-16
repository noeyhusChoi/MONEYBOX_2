using Device.Abstractions;
using Newtonsoft.Json;
using Pr22;
using Pr22.Events;
using Pr22.Imaging;
using Pr22.Processing;
using Pr22.Task;
using System.Diagnostics;
using System.IO;
using Path = System.IO.Path;

namespace Device.Devices
{
    public sealed class IdScannerDevice : IDevice
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
                    case "SCAN":
                        {
                            Debug.WriteLine("신분증 스캐너 시작");
                            return new(await ScanOnceAsync(TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(15), ct), "");
                        }

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
            // MRZ

            try
            {
                Pr22.Task.EngineTask OcrTask = new Pr22.Task.EngineTask();

                OcrTask.Add(FieldSource.Mrz, FieldId.All);      // MRZ fields

                var AnalyzeResult = _dev.Engine.Analyze(_page, OcrTask);

                List<Pr22.Processing.FieldReference> Fields = AnalyzeResult.GetFields();

                // MRZ
                if (Fields.Count > 0)
                {
                    FieldReference filter = new FieldReference(FieldSource.All, FieldId.Nationality);
                    List<FieldReference> tempFields = AnalyzeResult.GetFields(filter);

                    Debug.WriteLine($"{AnalyzeResult.GetField(FieldSource.Mrz, FieldId.Nationality).GetBestStringValue(),-16}");
                    Debug.WriteLine($"{AnalyzeResult.GetField(FieldSource.Mrz, FieldId.Name).GetBestStringValue(),-16}");
                    Debug.WriteLine($"{AnalyzeResult.GetField(FieldSource.Mrz, FieldId.DocumentNumber).GetBestStringValue(),-16}");
                    Debug.WriteLine($"{AnalyzeResult.GetField(FieldSource.Mrz, FieldId.Sex).GetBestStringValue(),-16}");
                    Debug.WriteLine($"{AnalyzeResult.GetField(FieldSource.Mrz, FieldId.BirthDate).GetBestStringValue(),-16}");
                    Debug.WriteLine($"{AnalyzeResult.GetField(FieldSource.Mrz, FieldId.Givenname).GetBestStringValue(),-16}");
                    Debug.WriteLine($"{AnalyzeResult.GetField(FieldSource.Mrz, FieldId.Surname).GetBestStringValue(),-16}");
                    Debug.WriteLine($"{AnalyzeResult.GetField(FieldSource.Mrz, FieldId.ExpiryDate).GetBestStringValue(),-16}");
                }
                // moneybox_ocr
                else
                {
                    // 스캔 이미지 = OCR/Input 생성
                    // OCR 진행 트리거 = OCR/Input  경로에 .ocr 파일 생성
                    // 스캔 결과 = OCR/Reult
                    // 스캔 결과(신분증타입) = 
                    // 분석

                    Debug.WriteLine("New OCR Start");

                    string szSessionID = "0101";

                    // 읽을 파일
                    string defaultPath = @"C:\Users\niaci\OneDrive\Dokumen\MoneyBox\SourceCode\MPOS_V2\Money24h\Bin\OCR";
                    string szResultTypeFilePath = Path.Combine(defaultPath, "resultType", $"{szSessionID}_Infra.json");
                    string szResultFilePath = Path.Combine(defaultPath, "result", $"{szSessionID}_Infra.json");

                    // 스캔 대상 이미지
                    try
                    {

                        var copyImgPath = Path.Combine(defaultPath, "input");
                        var img = _page.Select(Light.Infra).GetImage();
                        var fileName = $"{szSessionID}_{Light.Infra}.jpg";
                        var path = Path.Combine(copyImgPath, fileName);

                        img.Save(RawImage.FileFormat.Jpeg).Save(path);
                    }
                    catch
                    {
                        Debug.WriteLine("스캔 대상 이미지 생성 실패");
                    }


                    await Task.Delay(500);

                    try
                    {

                        string szOCRFilePath = Path.Combine(defaultPath, "input", $"{szSessionID}_Infra.ocr");

                        using (StreamWriter writer = new StreamWriter(szOCRFilePath, append: false))
                        {
                            writer.WriteLine("");
                        }

                    }
                    catch
                    {
                        Debug.WriteLine("트리거 파일 생성 실패");
                    }

                    await Task.Delay(500);

                    using (FileStream fs = new FileStream(szResultTypeFilePath, FileMode.Open, FileAccess.Read, FileShare.None))
                    using (StreamReader sr = new StreamReader(fs))
                    {
                        string szJsonDta = sr.ReadToEnd();

                        try
                        {
                            // JSON 데이터를 객체로 변환
                            JsonData data = JsonConvert.DeserializeObject<JsonData>(szJsonDta);


                            // 데이터 출력
                            Debug.WriteLine($"Authentication FilePath : {szResultTypeFilePath}");
                            Debug.WriteLine($"Type : {data.type}");

                            Debug.WriteLine($"===============================================DONE");
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine("Json 파일 읽기 실패");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
            }

            // Extern OCR


            return false;
        }


        private async Task<bool> ProcessOCR(string xx)
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

        public async Task<bool> ScanOnceAsync(TimeSpan? timeout = null, TimeSpan? removalTimeout = null, CancellationToken ct = default)
        {
            if (_dev == null) throw new InvalidOperationException("장비가 연결되어 있지 않습니다.");
            var effectiveTimeout = timeout ?? TimeSpan.FromSeconds(10);

            var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

            void OnPresence(object? s, DetectionEventArgs e)
            {
                try
                {
                    switch (e.State)
                    {
                        case Pr22.Util.PresenceState.Empty:
                            ScanSequence?.Invoke(this, ScanEvent.Empty);
                            break;
                        case Pr22.Util.PresenceState.Moving:
                            break;
                        case Pr22.Util.PresenceState.Present:
                        case Pr22.Util.PresenceState.NoMove:
                            ScanSequence?.Invoke(this, ScanEvent.Scanning);
                            tcs.TrySetResult(true);
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("OnPresence 처리 예외: " + ex);
                }
            }

            Pr22.Task.TaskControl? detection = null;

            _dev.PresenceStateChanged += OnPresence;

            try
            {
                try
                {
                    detection = _dev.Scanner.StartTask(FreerunTask.Detection());
                }
                catch (Exception ex)
                {
                    tcs.TrySetException(new InvalidOperationException("Detection start failed: " + ex.Message, ex));
                }

                using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
                var delayTask = Task.Delay(effectiveTimeout, timeoutCts.Token);

                var finished = await Task.WhenAny(tcs.Task, delayTask).ConfigureAwait(false);

                if (finished == tcs.Task)
                {
                    string? savedPath = null;
                    try
                    {
                        var task = new DocScannerTask() { };
                        task.Add(Light.White).Add(Light.Infra);
                        Pr22.Processing.Page page = _dev.Scanner.Scan(task, PagePosition.First);

                        var saveDir = Path.Combine(Environment.CurrentDirectory, "ScanOutput");
                        Directory.CreateDirectory(saveDir);

                        var img = page.Select(Light.White).GetImage();
                        var fileName = $"page0_light_{Light.White}_{DateTime.Now:yyyyMMdd_HHmmssfff}.png";
                        var path = Path.Combine(saveDir, fileName);

                        int attempts = 0;
                        bool saved = false;
                        while (!saved && attempts < 2 && !ct.IsCancellationRequested)
                        {
                            attempts++;
                            try
                            {
                                img.Save(RawImage.FileFormat.Png).Save(path);
                                saved = true;
                                savedPath = path;
                                ImageSaved?.Invoke(this, (0, Light.White, path));
                                Debug.WriteLine("Saved image: " + path);
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine($"이미지 저장 실패 (시도 {attempts}): {ex.Message}");
                                await Task.Delay(200, ct).ConfigureAwait(false);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("Scan/Save 처리중 예외: " + ex);
                    }

                    // 저장 후 ScanComplete 이벤트 (회수 대기는 하지 않음 — ViewModel에서 제어)
                    ScanSequence?.Invoke(this, ScanEvent.ScanComplete);

                    timeoutCts.Cancel();
                    var result = await tcs.Task.ConfigureAwait(false);
                    return result;
                }
                else
                {
                    Debug.WriteLine("감지 타임아웃");
                    return false;
                }
            }
            finally
            {
                try
                {
                    _dev.PresenceStateChanged -= OnPresence;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("이벤트 해제 중 예외: " + ex);
                }

                try
                {
                    if (detection != null)
                    {
                        var stopTask = Task.Run(() => detection.Stop());
                        if (!stopTask.Wait(TimeSpan.FromSeconds(2)))
                        {
                            Debug.WriteLine("detection.Stop() timed out");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("detection Stop 중 예외: " + ex);
                }
            }
        }
    }
}
