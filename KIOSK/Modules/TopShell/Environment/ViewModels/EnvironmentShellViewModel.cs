using CommunityToolkit.Mvvm.ComponentModel;
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
    public partial class EnvironmentShellViewModel : ObservableObject, ITopShellHost
    {
        private readonly INavigationService _nav;
        private readonly IInactivityService _inactivityService;

        [ObservableProperty]
        private object currentSubShell;

        [ObservableProperty]
        private object footerViewModel;

        [ObservableProperty]
        private object? popupContent;

        public EnvironmentShellViewModel(INavigationService nav, IInactivityService inactivityService, FooterViewModel footerViewModel)
        {
            _nav = nav;
        }

        public void SetSubShell(object? shell)
        {
            CurrentSubShell = shell;
        }

        public async Task OnLoadAsync(object? parameter, CancellationToken ct)
        {
            await Task.CompletedTask;
        }

        public async Task OnUnloadAsync()
        {
            await Task.CompletedTask;

        }
    }
}

