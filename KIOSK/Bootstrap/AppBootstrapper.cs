// AppBootstrapper.cs (refactored)
using KIOSK.Bootstrap.Modules;
using KIOSK.DataBase.Stores;
using KIOSK.Device.Core;
using KIOSK.Devices.Management;
using KIOSK.Models;
using KIOSK.Services;
using KIOSK.Services.DataBase;
using KIOSK.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace KIOSK.Bootstrap;

// TODO: Bootstrap 코드 정리 ( 레이어별 분류 )
public class AppBootstrapper : IDisposable
{
    private readonly IHost _host;
    public IServiceProvider ServiceProvider => _host.Services;

    public AppBootstrapper()
    {
        _host = Host.CreateDefaultBuilder()
            .ConfigureServices((ctx, services) =>
            {
                //// Manager
                //services.AddSingleton<DeviceManager>();
                
                // Manager V2
                services.AddSingleton<IDeviceManager, DeviceManagerV2>();
                services.AddSingleton<IDeviceStatusStore, DeviceStatusStore>();
                services.AddSingleton<IDeviceCommandBus, DeviceCommandBus>();
                services.AddSingleton<IDeviceRuntime, DeviceRuntime>();

                // Store
                services.AddSingleton<KioskStore>();
                services.AddSingleton<DeviceStore>();

                // Model
                services.AddSingleton<ExchangeRateModel>();

                // View Models
                services.AddViewModels();

                // Services
                services.AddServices();

                // StateMachines
                services.AddStateMachines();

                // Background Tasks
                services.AddBackgroundServices();

                // HostedService 등록
                services.AddHostedService<BackgroundTaskService>();

                // 기타: View/Window는 App에서 직접 new 해도 괜찮지만 DI로 관리 가능
                services.AddSingleton<MainWindow>();
            })
            .ConfigureLogging((ctx, logging) =>
            {
                logging.ClearProviders();
#if DEBUG
                //logging.AddDebug();
#endif
            })
            .Build();
    }

    public async Task StartAsync()
    {
        await _host.StartAsync();

        var _logging = ServiceProvider.GetRequiredService<ILoggingService>();
        _logging.Info("App host started.");

        // 초기화 서비스 실행
        var initializeService = ServiceProvider.GetRequiredService<IBootstrapService>();
        await initializeService.initializeAsync();

        // MainWindow 띄우기
        var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
        mainWindow.DataContext = ServiceProvider.GetRequiredService<MainViewModel>();
        mainWindow.Show();
    }

    public async Task StopAsync()
    {
        await _host.StopAsync();
    }

    public void Dispose() => _host.Dispose();
}
