using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KIOSK.Device.Abstractions;
using KIOSK.Devices.Management;
using KIOSK.Services;
using KIOSK.Services.API;
using System.Collections.ObjectModel;

namespace KIOSK.ViewModels.GTF
{
    public partial class GtfRefundVoucherRegisterViewModel : ObservableObject, IStepMain, IStepNext, IStepPrevious, IStepError, INavigable
    {
        private readonly IDeviceManager _deviceManager;
        private readonly GtfApiService _gtfApiService;
        private readonly IGtfTaxRefundService _gtfTaxRefundService;

        public class VoucherRow
        {
            public string CurrencyCode { get; set; } = "";
            public int Denomination { get; set; }
            public int Count { get; set; }
            public int Amount { get; set; }
        }

        public ObservableCollection<VoucherRow> TempClass { get; }
            = new()
            {
            new VoucherRow
            {
                CurrencyCode = "홍길동",
                Denomination = 2,
                Count = 100000,
                Amount = 7000
            }
            };

        public Func<Task>? OnStepMain { get; set; }
        public Func<Task>? OnStepPrevious { get; set; }
        public Func<string?, Task>? OnStepNext { get; set; }
        public Action<Exception>? OnStepError { get; set; }

        public GtfRefundVoucherRegisterViewModel(IDeviceManager deviceManager, GtfApiService gtfApiService, IGtfTaxRefundService gtfTaxRefundService)
        {
            _deviceManager = deviceManager;
            _gtfApiService = gtfApiService;
            _gtfTaxRefundService = gtfTaxRefundService;
        }

        public async Task OnLoadAsync(object? parameter, CancellationToken ct)
        {
            // TODO: 로딩 시 필요한 작업 수행
        }

        public async Task OnUnloadAsync()
        {
            // TODO: 언로드 시 필요한 작업 수행
            await _deviceManager.SendAsync("QR1", new DeviceCommand("SCAN_DISABLE"));
        }

        public async Task InitialAsync()
        {
            // -- 전체 로직
            // 바우처 QR 인식
            // 인식 데이터 파싱
            // API 호출 및 결과 처리
            // 바우처 정보 표시
            // 오류 처리

            // -- 해당 메서드
            // QR 스캐너 동작 시작
            await _deviceManager.SendAsync("QR1", new DeviceCommand("SCAN_ENABLE"));
        }

        // QR 코드 스캔 처리 메서드
        private async Task ScanVoucherQrCodeAsync(CancellationToken ct)
        {
            // QR 코드 스캔 로직 구현
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
        private async Task Next(object? parameter)
        {
            try
            {
                if (OnStepNext is not null)
                    await OnStepNext("");
            }
            catch (Exception ex)
            {
                OnStepError?.Invoke(ex);
            }
        }
        #endregion
    }
}
