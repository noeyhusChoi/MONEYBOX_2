using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Device.Abstractions;
using Device.Core;
using KIOSK.Services;
using KIOSK.ViewModels.Exchange.Popup;
using System.Diagnostics;

namespace KIOSK.ViewModels
{
    public partial class ExchangeIDScanViewModel : ObservableObject, IStepMain, IStepNext, IStepPrevious, IStepError
    {
        private readonly IDialogService _dialog;        // TEST
        private readonly DeviceManager _deviceManager;

        public Func<Task>? OnStepMain { get; set; }
        public Func<Task>? OnStepPrevious { get; set; }
        public Func<bool?, Task>? OnStepNext { get; set; }
        public Action<Exception>? OnStepError { get; set; }

        public ExchangeIDScanViewModel(DeviceManager deviceManager, IDialogService dialog)
        {
            _deviceManager = deviceManager;
            _dialog = dialog;
        }

        [RelayCommand]
        private async Task Loaded(object parameter) // 파라미터 필요없으면 object 대신 없음
        {
            // 팝업 & 스캐너 인식


            // 1) 스캔을 백그라운드로 시작
            var scanTask = Task.Run(async () =>
            {
                try
                {
                    int maintainCount = 0;

                    // DeviceManager.SendAsync가 내부에서 비동기라면 Task.Run 불필요합니다.
                    while (true)
                    {
                        var res = await _deviceManager.SendAsync("IDSCANNER1", new DeviceCommand("ScanStart"));

                        if (res != null && res.Success == true)
                        {
                            res = await _deviceManager.SendAsync("IDSCANNER1", new DeviceCommand("GetScanStatus"));


                            switch ((Pr22.Util.PresenceState)res.Data)
                            {
                                case Pr22.Util.PresenceState.Empty:
                                    maintainCount = 0;
                                    Debug.WriteLine("Scan Empty");
                                    break;

                                case Pr22.Util.PresenceState.Dirty:
                                case Pr22.Util.PresenceState.Moving:
                                    maintainCount = 0;
                                    Debug.WriteLine("Scan Moving");
                                    break;

                                case Pr22.Util.PresenceState.Present:
                                case Pr22.Util.PresenceState.NoMove:
                                    Debug.WriteLine("Scan Detected ");
                                    
                                    if (++maintainCount > 2)
                                    {
                                        Debug.WriteLine("Scan Complete ");
                                        await _deviceManager.SendAsync("IDSCANNER1", new DeviceCommand("ScanStop"));
                                        var result = await _deviceManager.SendAsync("IDSCANNER1", new DeviceCommand("SaveImage"));

                                        return res;
                                    }
                                    break;
                            }
                        }

                        await Task.Delay(200);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Scan failed: " + ex);
                    throw;
                }
            });

            var vm = new ConfirmDialogViewModel();
            Task<bool> dialogTask = _dialog.ShowDialogAsync<bool>(vm);

            // 3) 둘을 동시에 기다리거나, 우선순위에 따라 처리
            var completed = await Task.WhenAny(scanTask, dialogTask);

            // 스캔이 먼저 끝남 — 결과 처리
            if (completed == scanTask)
            {
                var scanResult = await scanTask; // TODO: 예외 발생 가능, try/catch

                if (scanResult.Success == true)
                {
                    vm.RequestCloseFromCaller(default);
                    await Task.Delay(200);
                    await Next(true);
                    Debug.WriteLine("스캔 성공");
                }

                Debug.WriteLine("Scan finished while popup open");
            }
            else
            {
                Debug.WriteLine("Popup closed before scan finished");
            }
        }

        [RelayCommand]
        private async Task Main()
        {
            try
            {
                OnStepMain?.Invoke();
            }
            catch (Exception ex)
            {
                OnStepError?.Invoke(ex);
            }
        }

        [RelayCommand]
        private async Task Previous()
        {
            try
            {
                OnStepPrevious?.Invoke();
            }
            catch (Exception ex)
            {
                OnStepError?.Invoke(ex);
            }
        }

        [RelayCommand]
        private async Task Next(object? o)
        {
            try
            {
#if DEBUG
                OnStepNext?.Invoke(true);
#else
                return;
#endif
            }
            catch (Exception ex)
            {
                OnStepError?.Invoke(ex);
            }
        }
    }
}
