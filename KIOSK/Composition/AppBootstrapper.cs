// AppBootstrapper.cs (refactored)
using KIOSK.Composition.Modules;
using KIOSK.Infrastructure.Hosting;
using KIOSK.Infrastructure.Initialization;
using KIOSK.Infrastructure.Logging;
using KIOSK.Services;
using KIOSK.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace KIOSK.Composition;

// TODO: Bootstrap 코드 정리 ( 레이어별 분류 )
public class AppBootstrapper : IDisposable
{
    private readonly IHost _host;
    
    public IServiceProvider _serviceProvider => _host.Services;
    public AppBootstrapper()
    {
        _host = Host.CreateDefaultBuilder()
            .ConfigureServices((ctx, services) =>
            {
                services.AddAppModules();

                // HostedService 등록
                services.AddHostedService<BackgroundTaskService>();

                // 기타: View/Window는 App에서 직접 new 해도 괜찮지만 DI로 관리 가능
                services.AddSingleton<MainWindowView>();
            })
            .ConfigureLogging((ctx, logging) =>
            {
                logging.ClearProviders();
            })
            .Build();
    }

    public async Task StartAsync()
    {
        var logger = _serviceProvider.GetRequiredService<ILoggingService>();
        var initializer = _serviceProvider.GetRequiredService<IAppInitializer>();

        // 1) 초기화 실행
        await initializer.InitializeAsync().ConfigureAwait(false);

        // 2) HostedService 등 실행
        await _host.StartAsync().ConfigureAwait(false);
        logger.Info("App host started.");

        // 3) 화면 표시
        var mainWindow = _serviceProvider.GetRequiredService<MainWindowView>();
        mainWindow.DataContext = _serviceProvider.GetRequiredService<MainWindowViewModel>();
        mainWindow.Show();

        logger.Info("App display started");
    }

    public async Task StopAsync()
    {
        await _host.StopAsync();
    }

    public void Dispose() => _host.Dispose();
}
