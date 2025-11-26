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
using System.Diagnostics;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace KIOSK.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly IServiceProvider _provider;

    [ObservableProperty]
    private object rootViewModel; // MainWindow의 Content

    public MainViewModel(IServiceProvider provider)
    {
        _provider = provider;
        
        RootViewModel = _provider.GetRequiredService<LoadingViewModel>();
    }

    public async Task InitializeAsync()
    {
        // 장치 초기화 로직
        //var deviceManager = _provider.GetRequiredService<DeviceManager>();
        //var descriptors = _provider.GetRequiredService<IEnumerable<DeviceDescriptor>>();
        //foreach (var d in descriptors)
        //    await deviceManager.AddAsync(d);

        await Task.Delay(2000); // 시뮬레이션용 딜레이
       
        // 준비 완료 후: 실제 쉘로 교체
        RootViewModel = _provider.GetRequiredService<MainShellViewModel>();
    }

    [RelayCommand]
    private async void ChangeMonitor()
    {
#if DEBUG
        MonitorMover.MoveActiveWindowToNextScreen();
        //CancellationToken tt = new();

        //var gtf = _provider.GetRequiredService<GtfApiService>();
        //var dto = new InitialRequestDto() { Edi = "1", ShopName = "2", TmlId = "3" };
        //var result = await gtf.InitialAsync(dto, tt);
#endif
    }

    [RelayCommand]
    private async void Withdrawal()
    {
#if DEBUG
        // 준비 완료 후: 실제 쉘로 교체
        if(RootViewModel is EnvironmentViewModel)
            RootViewModel = _provider.GetRequiredService<MainShellViewModel>();
        else
            RootViewModel = _provider.GetRequiredService<EnvironmentViewModel>();

#endif
    }


    [RelayCommand]
    private async void QrOn()
    {
#if DEBUG
        var device = _provider.GetRequiredService<IDeviceManager>();
        await device.SendAsync("QR1", new DeviceCommand("SCAN_ENABLE"));
#endif
    }

    [RelayCommand]
    private async void QrOff()
    {
#if DEBUG
        var device = _provider.GetRequiredService<IDeviceManager>();
        await device.SendAsync("QR1", new DeviceCommand("SCAN_DISABLE"));
#endif
    }
}

