// AppBootstrapper.cs (refactored)
using Device.Core;
using KIOSK.Bootstrap.Modules;
using KIOSK.Models;
using KIOSK.Services;
using KIOSK.Stores;
using KIOSK.ViewModels;
using KIOSK.ViewModels.Exchange.Popup;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.Json.Serialization;

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
                // Manager
                services.AddSingleton<DeviceManager>();

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


                // TEST
                services.AddSingleton<IDialogService, DialogService>();
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

        // 기존대로 InitializeService 실행(필요하면 IHostedService로 옮길 수도 있음)
        var initializeService = ServiceProvider.GetRequiredService<IInitializeService>();
        await initializeService.initialize();

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
