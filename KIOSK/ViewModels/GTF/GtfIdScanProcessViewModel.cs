using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KIOSK.Device.Abstractions;
using KIOSK.Devices.Management;
using KIOSK.Services.DataBase;
using Localization;
using Pr22.Processing;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WpfApp1.NewFolder;

namespace KIOSK.ViewModels.GTF
{
    public partial class GtfIdScanProcessViewModel : ObservableObject, IStepMain, IStepNext, IStepPrevious, IStepError, INavigable
    {
        public Func<Task>? OnStepMain { get; set; }
        public Func<Task>? OnStepPrevious { get; set; }
        public Func<bool?, Task>? OnStepNext { get; set; }
        public Action<Exception>? OnStepError { get; set; }

        private readonly IDeviceManager _deviceManager;
        private readonly IOcrService _ocrService;

        public GtfIdScanProcessViewModel(IDeviceManager deviceManager, IOcrService ocrService) 
        {
            _deviceManager = deviceManager;
            _ocrService = ocrService;
        }

        public Task OnLoadAsync(object? parameter, CancellationToken ct)
        {
            _ = Task.Run(() => InitAsync(ct), ct);
            return Task.CompletedTask;
        }

        public async Task OnUnloadAsync()
        {
            // TODO: 언로드 시 필요한 작업 수행
        }

        private async Task InitAsync(CancellationToken ct)
        {
            // 여기서는 ConfigureAwait(false)로 컨텍스트 캡처 방지
            //await _deviceManager.SendAsync("IDSCANNER1", new DeviceCommand("ScanStop"))
            //                    .WaitAsync(ct)
            //                    .ConfigureAwait(false);

            var result = await _deviceManager.SendAsync("IDSCANNER1", new DeviceCommand("SaveImage"))
                                             .WaitAsync(ct)
                                             .ConfigureAwait(false);

            if (result?.Data is Page page)
            {
                try
                {
                    var outcome = await _ocrService.RunAsync(page, OcrMode.Auto, CancellationToken.None)
                                                   .ConfigureAwait(false);

                    if (outcome.Success)
                    {
                        foreach (var value in outcome.Fields)
                            Trace.WriteLine($"{value}");

                        // 1) 스캔 원본 내부 리소스 해제 (Page가 IDisposable이면 dispose)
                        if (page is IDisposable d)
                            d.Dispose();

                        // 2) 해제 작업을 실행할 딜레이
                        await Task.Delay(50, ct).ConfigureAwait(false);

                        // UI 동작 Dispatcher로 넘기기
                        await App.Current.Dispatcher.InvokeAsync(async () =>
                        {
                            await Next(true);
                        });
                    }
                    else
                    {
                        await App.Current.Dispatcher.InvokeAsync(async () =>
                        {
                            await Previous();
                        });
                    }
                }
                catch (Exception ex)
                {
                    await App.Current.Dispatcher.InvokeAsync(() =>
                    {
                        OnStepError?.Invoke(ex);
                    });
                }
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
