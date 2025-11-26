using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KIOSK.Device.Abstractions;
using KIOSK.Device.Core;
using KIOSK.Devices.Management;
using KIOSK.Models;
using KIOSK.Services;
using KIOSK.Services.API;
using KIOSK.Views;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows.Input;

namespace KIOSK.ViewModels
{
    public partial class MainShellViewModel : ObservableObject
    {
        private readonly IServiceProvider _provider;
        
        private object _currentViewModel;
        public object CurrentViewModel
        {
            get => _currentViewModel;
            set
            {
                // 이전 ViewModel Dispose
                if (_currentViewModel is IDisposable disposable)
                    disposable.Dispose();

                SetProperty(ref _currentViewModel, null);   // 1. null 할당
                GC.WaitForPendingFinalizers();              // 주석하여 테스트 후 문제 없을 시 삭제 

                SetProperty(ref _currentViewModel, value);  // 2. 새로 할당
            }
        }

        [ObservableProperty]
        private object footerViewModel;


        public Action<object>? NavigateAction { get; set; }

        public MainShellViewModel(IServiceProvider provider)
        {
            _provider = provider;
            CurrentViewModel = _provider.GetRequiredService<ServiceViewModel>();
            FooterViewModel = _provider.GetRequiredService<FooterViewModel>();

            NavigateAction = vm => CurrentViewModel = vm;

            _idle = _provider.GetRequiredService<IInactivityService>(); // 전역 DI 등록 기준
        }

        private readonly IInactivityService _idle;

        [RelayCommand]
        private void RootInput()
        {
            _idle.Reset(); // 모든 터치 / 클릭
        }

        [RelayCommand]
        private void NavigateToHome()
        {
#if DEBUG
            var _nav = _provider.GetRequiredService<INavigationService>();
            _nav.NavigateTo<ServiceViewModel>();
#endif
        }

        [RelayCommand]
        private void ChangeMonitor()
        {
#if DEBUG
            //MonitorMover.MoveActiveWindowToNextScreen();
            var _nav = _provider.GetRequiredService<INavigationService>();
            _nav.NavigateTo<LoadingViewModel>();
#endif
        }

        [RelayCommand]
        private async void Withdrawal()
        {
#if DEBUG
            //var receiptService = _provider.GetRequiredService<ReceiptPrintService>();
            //await receiptService.PrintReceiptAsync("en-US", new TransactionModelV2());

            //var api = _provider.GetRequiredService<CemsApiService>();
            //var cassette = _provider.GetRequiredService<WithdrawalCassetteService>();

            //await cassette.InitializeAsync();
            //var result = await api.SetCashAsync(cassette.Get(), default);
#endif
        }



        [RelayCommand]
        private async void ON()
        {
#if DEBUG
            var device = _provider.GetRequiredService<IDeviceManager>();
            await device.SendAsync("QR1", new DeviceCommand("SCAN.TRIGGERON"));
#endif
        }

        [RelayCommand]
        private async void OFF()
        {
#if DEBUG

            var device = _provider.GetRequiredService<IDeviceManager>();
            await device.SendAsync("QR1", new DeviceCommand("SCAN.TRIGGEROFF"));

#endif
        }
    }
}
