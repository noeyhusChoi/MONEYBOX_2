// AppBootstrapper.cs (refactored)
using Device.Core;
using KIOSK.Bootstrap.Modules;
using KIOSK.Models;
using KIOSK.Services;
using KIOSK.Stores;
using KIOSK.ViewModels;
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
                services.AddSingleton<DeviceManager>();

                services.AddSingleton<KioskStore>();
                services.AddSingleton<DeviceStore>();

                services.AddSingleton<ExchangeRateModel>();

                services.AddViewModels();
                services.AddServices();
                services.AddStateMachines();

                // Background tasks
                services.AddSingleton(new BackgroundTaskDescriptor(
                    name: "Device_Status",
                    interval: TimeSpan.FromSeconds(10),
                    action: async (sp, ct) =>
                    {
                        // sp는 scope.ServiceProvider (DB 등 안전 사용)
                        var logger = sp.GetRequiredService<ILoggingService>();

                        var deviceManager = sp.GetRequiredService<DeviceManager>();
                        var snapshots = deviceManager.GetLatestSnapshots();

                        foreach (var snapshot in snapshots)
                        {
                            var joined = string.Join(", ", snapshot.Alarms?.Select(a => a.Message) ?? Enumerable.Empty<string>());

                            logger.Debug($"{snapshot.Name} / 포트:{snapshot.IsPortError} / 통신:{snapshot.IsCommError} / 에러:{joined}");
                        }

                        await Task.CompletedTask;
                    }));

                services.AddSingleton(new BackgroundTaskDescriptor(
                    name: "CurrencyRate Update",
                    interval: TimeSpan.FromSeconds(10),
                    action: async (sp, ct) =>
                    {
                        // sp는 scope.ServiceProvider (DB 등 안전 사용)
                        var logger = sp.GetRequiredService<ILoggingService>();

                        var x = sp.GetRequiredService<IApiService>();
                        var result = await x.SendCommandAsync("C011", null);

                        var options = new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true,
                            NumberHandling = JsonNumberHandling.AllowReadingFromString
                        };

                        var model = sp.GetRequiredService<ExchangeRateModel>();
                        var response = JsonSerializer.Deserialize<ExchangeRateModel>(result, options);
                        model.Result = response.Result;
                        model.Data = response.Data;

                        await Task.CompletedTask;
                    }));

                // HostedService 등록
                services.AddHostedService<BackgroundTaskService>();

                // 기타: View/Window는 App에서 직접 new 해도 괜찮지만 DI로 관리 가능
                services.AddSingleton<MainWindow>();
            })
            .ConfigureLogging((ctx, logging) =>
            {
                logging.ClearProviders();
                logging.AddDebug();
                logging.AddConsole();
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
