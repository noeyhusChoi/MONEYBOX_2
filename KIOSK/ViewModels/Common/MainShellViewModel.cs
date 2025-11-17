using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KIOSK.Models;
using KIOSK.Services;
using KIOSK.Services.API;
using KIOSK.Views;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Text.Json.Serialization;

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
    }
}
