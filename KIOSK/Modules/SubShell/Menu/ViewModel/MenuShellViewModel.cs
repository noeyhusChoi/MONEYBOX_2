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
    public partial class MenuSubShellViewModel : ObservableObject, ISubShellHost
    {
        private readonly INavigationService _nav;

        public MenuSubShellViewModel(INavigationService nav)
        {
            _nav = nav;
        }

        [ObservableProperty]
        private object? currentView;

        public void SetInnerView(object view)
        {
            CurrentView = view;
        }

        [ObservableProperty]
        private object? popupContent;

        public async Task OnLoadAsync(object? parameter, CancellationToken ct)
        {
            await _nav.NavigateTo<ServiceViewModel>();
        }

        public async Task OnUnloadAsync()
        {
            await Task.CompletedTask;
        }
    }
}
