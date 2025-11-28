using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KIOSK.Modules.GTF.Shell;
using KIOSK.Modules.Shells.Interface;
using KIOSK.Services;
using KIOSK.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KIOSK.ViewModels
{
    public partial class MenuShellViewModel : ObservableObject, IShellHost
    {
        private readonly INavigationService _nav;

        public MenuShellViewModel(INavigationService nav)
        {
            _nav = nav;
        }

        [ObservableProperty]
        private object? currentView;

        public void SetInnerView(object view)
        {
            CurrentView = view;
        }

        public async Task OnLoadAsync(object? parameter, CancellationToken ct)
        {
            await Task.CompletedTask;
        }

        public async Task OnUnloadAsync()
        {
            await Task.CompletedTask;
        }

        // Local Popup
        [ObservableProperty]
        private object? localPopupViewModel;

        [ObservableProperty]
        private bool isLocalPopupOpen;

        // RootShell PopupService가 호출하는 Local Popup Close
        public void CloseLocalPopup()
        {
            IsLocalPopupOpen = false;
            LocalPopupViewModel = null;
        }

        [RelayCommand]
        private async Task GoAsync(object parameter)
        {
            if (parameter is string exchangeType)
            {
                switch (exchangeType.ToUpper())
                {
                    case "MENU":
                        await _nav.SwitchSubShell<MenuShellViewModel>();
                        break;
                    case "EXCHANGE":
                        //await _nav.SwitchSubShell<ExchangeShellViewModel>();
                        break;
                    case "GTF":
                        await _nav.SwitchSubShell<GtfShellViewModel>();
                        break;
                }
            }
        }
    }
}
