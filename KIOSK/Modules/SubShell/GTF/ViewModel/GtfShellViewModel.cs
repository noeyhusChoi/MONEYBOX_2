using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KIOSK.Modules.GTF.ViewModels;
using KIOSK.Modules.Shells.Interface;
using KIOSK.Services;
using KIOSK.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KIOSK.Modules.GTF.Shell
{
    public partial class GtfShellViewModel : ObservableObject, IShellHost
    {
        private readonly INavigationService _nav;

        public GtfShellViewModel(INavigationService nav)
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
    }
}
