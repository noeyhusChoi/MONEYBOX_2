using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KIOSK.Device.Abstractions;
using KIOSK.Device.Core;
using KIOSK.Devices.Management;
using KIOSK.Services;
using KIOSK.ViewModels.Exchange.Popup;
using System.Diagnostics;
using WpfApp1.NewFolder;

namespace KIOSK.ViewModels
{
    public partial class ExchangeIDScanGuideViewModel : ObservableObject, IStepMain, IStepNext, IStepPrevious, IStepError, INavigable
    {
        private readonly IDeviceManager _deviceManager;
        private readonly IOcrService _ocr;
        private readonly IDialogService _dialog;        // TEST
        private readonly ExchangePopupIDScanInfoViewModel _popup;

        public Func<Task>? OnStepMain { get; set; }
        public Func<Task>? OnStepPrevious { get; set; }
        public Func<bool?, Task>? OnStepNext { get; set; }
        public Action<Exception>? OnStepError { get; set; }

        public ExchangeIDScanGuideViewModel(IDeviceManager deviceManager, IDialogService dialog, IOcrService ocr, ExchangePopupIDScanInfoViewModel popup)
        {
            _deviceManager = deviceManager;
            _ocr = ocr;
            _dialog = dialog;
            _popup = popup;
        }

        public async Task OnLoadAsync(object? parameter, CancellationToken ct)
        {
            using var scanCts = new CancellationTokenSource();

            var scanTask = ScanUntilStableAsync(scanCts.Token);
            var dialogTask = _dialog.ShowDialogAsync<bool>(_popup);

            // 10초 동안만 스캔 완료 기다리기
            var completed = await Task.WhenAny(scanTask, Task.Delay(10000));

            if (completed == scanTask)
            {
                // 스캔 성공 브랜치
                CommandResult scanResult;
                try
                {
                    scanResult = await scanTask; // 예외 전파
                }
                catch (OperationCanceledException)
                {
                    scanResult = new CommandResult(false);
                    return;
                }

                if (scanResult.Success == true)
                {
                    _popup?.RequestCloseFromCaller();
                    await Task.Delay(200);
                    await Next(true);
                }
            }
            else
            {
                // 스캔 루프 중단 + 필요시 Stop 명령
                scanCts.Cancel();

                try
                {
                    await scanTask; // 취소 대기
                }
                catch (OperationCanceledException)
                {
                    // 무시
                }

                try
                {
                    await _deviceManager
                        .SendAsync("IDSCANNER1", new DeviceCommand("ScanStop"))
                        .WaitAsync(TimeSpan.FromMilliseconds(500));
                }
                catch { }

                _popup?.RequestCloseFromCaller();
                await Task.Delay(200);
                await Previous();
            }
        }

        public async Task OnUnloadAsync()
        {
            await _deviceManager.SendAsync("IDSCANNER1", new DeviceCommand("ScanStop"));
            // TODO: 언로드 시 필요한 작업 수행
        }

        private async Task<CommandResult?> ScanUntilStableAsync(CancellationToken ct)
        {
            int maintainCount = 0;

            while (true)
            {
                ct.ThrowIfCancellationRequested();

                // SendAsync가 ct를 받지 못하면 .WaitAsync(ct)로 감싸기
                var res = await _deviceManager
                    .SendAsync("IDSCANNER1", new DeviceCommand("ScanStart"))
                    .WaitAsync(ct);

                if (res == null || res.Success == false)
                {
                    res = await _deviceManager
                    .SendAsync("IDSCANNER1", new DeviceCommand("ScanStart"))
                    .WaitAsync(ct);
                }
                else
                {
                    var status = await _deviceManager
                        .SendAsync("IDSCANNER1", new DeviceCommand("GetScanStatus"))
                        .WaitAsync(ct);

                    switch ((Pr22.Util.PresenceState)status?.Data)
                    {
                        case Pr22.Util.PresenceState.Empty:
                        case Pr22.Util.PresenceState.Dirty:
                        case Pr22.Util.PresenceState.Moving:
                            Trace.WriteLine("count = 0");
                            maintainCount = 0;
                            break;

                        case Pr22.Util.PresenceState.Present:
                        case Pr22.Util.PresenceState.NoMove:
                            Trace.WriteLine($"Nomove = 0 {maintainCount}|");
                            if (++maintainCount > 5)
                                return status;
                            break;
                    }
                }

                await Task.Delay(200, ct);
            }
        }

        #region Commands
        [RelayCommand]
        private async Task Main()
        {
            try
            {
                if (OnStepMain is not null)
                    await OnStepMain();
            }
            catch (Exception ex)
            {
                if (OnStepError is not null)
                    OnStepError(ex);
            }
        }

        [RelayCommand]
        private async Task Previous()
        {
            try
            {
                if (OnStepPrevious is not null)
                    await OnStepPrevious();
            }
            catch (Exception ex)
            {
                if (OnStepError is not null)
                    OnStepError(ex);
            }
        }

        [RelayCommand]
        private async Task Next(object? o)
        {
            try
            {
                if (OnStepNext is not null)
                    await OnStepNext(true);
            }
            catch (Exception ex)
            {
                if (OnStepError is not null)
                    OnStepError(ex);
            }
        }
        #endregion
    }
}
