using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KIOSK.Device.Abstractions;
using KIOSK.Device.Core;
using KIOSK.Devices.Management;
using KIOSK.Models;
using KIOSK.Modules.Shells.Interface;
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
    public partial class RootShellViewModel : ObservableObject, ITopShellHost
    {
        private readonly INavigationService _nav;
        private readonly IInactivityService _inactivityService;

        [ObservableProperty]
        private object currentSubShell;

        [ObservableProperty]
        private object footerViewModel;

        [ObservableProperty]
        private object? popupContent;

        public RootShellViewModel(INavigationService nav, IInactivityService inactivityService, FooterViewModel footerViewModel)
        {
            _nav = nav;
            _inactivityService = inactivityService; // Update to use injected service

            //CurrentViewModel = _nav.GetRequiredService<MenuViewModel>();
            
            FooterViewModel = footerViewModel; // 푸터 고정
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
