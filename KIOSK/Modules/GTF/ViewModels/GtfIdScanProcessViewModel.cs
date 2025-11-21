using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KIOSK.API.GTF.KIOSK.API.Gtf;
using KIOSK.Device.Abstractions;
using KIOSK.Devices.Management;
using KIOSK.Services;
using KIOSK.Services.API;
using Pr22.Processing;
using System.Diagnostics;
using WpfApp1.NewFolder;

namespace KIOSK.ViewModels.GTF
{
    public partial class GtfIdScanProcessViewModel : ObservableObject, IStepMain, IStepNext, IStepPrevious, IStepError, INavigable
    {
        public Func<Task>? OnStepMain { get; set; }
        public Func<Task>? OnStepPrevious { get; set; }
        public Func<string?, Task>? OnStepNext { get; set; }
        public Action<Exception>? OnStepError { get; set; }

        private readonly IDeviceManager _deviceManager;
        private readonly IOcrService _ocrService;
        private readonly GtfApiService _gtfApiService;
        private readonly IGtfTaxRefundService _gtfTaxRefundService;

        public GtfIdScanProcessViewModel(IDeviceManager deviceManager, IOcrService ocrService, GtfApiService gtfApiService, IGtfTaxRefundService gtfTaxRefundService)
        {
            _deviceManager = deviceManager;
            _ocrService = ocrService;
            _gtfApiService = gtfApiService;
            _gtfTaxRefundService = gtfTaxRefundService;
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
            try
            {
                if (ct.IsCancellationRequested)
                    return;

                // ID 스캐너로 이미지 캡처
                var result = await _deviceManager
                    .SendAsync("IDSCANNER1", new DeviceCommand("SaveImage"))
                    .WaitAsync(ct)
                    .ConfigureAwait(false);

                if (ct.IsCancellationRequested)
                    return;

                if (result?.Data is not Page page)
                {
                    // 스캔 실패 → 이전 화면으로
                    await App.Current.Dispatcher.InvokeAsync(async () => { await Previous(); });
                    return;
                }

                try
                {
                    // OCR 수행
                    var outcome = await _ocrService
                        .RunAsync(page, OcrMode.Auto, CancellationToken.None)
                        .ConfigureAwait(false);

                    if (!outcome.Success)
                    {
                        await App.Current.Dispatcher.InvokeAsync(async () => { await Previous(); });
                        return;
                    }

                    // 디버깅용 필드 출력
                    foreach (var kv in outcome.Fields)
                        Trace.WriteLine($"{kv.Key} = {kv.Value}");

                    // OCR 결과 파싱 (필드 누락 대비 TryGetValue 사용)
                    if (!outcome.Fields.TryGetValue("BirthDate", out var birthDate) ||
                        !outcome.Fields.TryGetValue("Sex", out var sex) ||
                        !outcome.Fields.TryGetValue("NAME", out var name) ||
                        !outcome.Fields.TryGetValue("NATIONALITY", out var nationality) ||
                        !outcome.Fields.TryGetValue("ExpiryDate", out var expiryDate) ||
                        !outcome.Fields.TryGetValue("NO", out var passportNo))
                    {
                        // 필수 필드 누락 → 에러 처리 또는 이전 화면
                        await App.Current.Dispatcher.InvokeAsync(async () => { await Previous(); });
                        return;
                    }

                    var req = new InquirySlipListRequestDto
                    {
                        KioskNo = _gtfTaxRefundService.Current.KioskNo,
                        KioskType = _gtfTaxRefundService.Current.KioskType,
                        Birthday = DateTime.TryParse(birthDate, null, out var birthDt) ? birthDt.ToString("yyMMdd") : string.Empty,
                        GenderCode = sex,
                        Name = name,
                        NationalityCode = nationality,
                        PassportExpirdate = DateTime.TryParse(expiryDate, null, out var expiryDt) ? expiryDt.ToString("yyMMdd") : string.Empty,
                        PassportNo = passportNo,
                        InputWayCode = "02",
                    };

                    var res = await _gtfApiService.InquirySlipListAsync(req, ct)
                                                  .ConfigureAwait(false);

                    //    "0000" = 정상 → 이때 세션에 반영
                    if (res.Rc == "0000")
                    {
                        _gtfTaxRefundService.ApplyInquirySlipList(req, res);

                        // 약간의 딜레이 후 화면 전환
                        await Task.Delay(50, ct).ConfigureAwait(false);

                        await App.Current.Dispatcher.InvokeAsync(() =>
                        {
                            if (OnStepNext is not null)
                                return OnStepNext("");

                            return Task.CompletedTask;
                        });
                    }
                    else
                    {
                        // 오류 코드 → 이전 화면 or 에러 화면
                        await App.Current.Dispatcher.InvokeAsync(async () => { await Previous(); });
                    }
                }
                finally
                {
                    // Page 리소스 해제
                    if (result?.Data is IDisposable disposable)
                        disposable.Dispose();
                }
            }
            catch (OperationCanceledException)
            {
                // 취소
            }
            catch (Exception ex)
            {
                await App.Current.Dispatcher.InvokeAsync(() =>
                {
                    OnStepError?.Invoke(ex);
                });
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
                    await OnStepNext("");
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
