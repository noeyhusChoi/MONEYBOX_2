using Device.Core;
using KIOSK.FSM;
using KIOSK.FSM.MOCK;
using KIOSK.Models;
using KIOSK.Services;
using KIOSK.ViewModels;
using KIOSK.ViewModels.Exchange.Popup;
using Localization;
using Microsoft.Extensions.DependencyInjection;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using WpfApp1.NewFolder;

namespace KIOSK.Bootstrap.Modules;

public static class BootstrapExtensions
{
    public static IServiceCollection AddViewModels(this IServiceCollection services)
    {
        services.AddSingleton<FooterViewModel>();
        services.AddSingleton<MainViewModel>();

        services.AddTransient<ServiceViewModel>();
        services.AddSingleton<LoadingViewModel>();

        services.AddTransient<ExchangeLanguageViewModel>();
        services.AddTransient<ExchangeCurrencyViewModel>();
        services.AddTransient<ExchangeTermsViewModel>();
        services.AddTransient<ExchangeIDScanViewModel>();
        services.AddTransient<ExchangeIDScanningViewModel>();
        services.AddTransient<ExchangeIDScanCompleteViewModel>();
        services.AddTransient<ExchangeDepositViewModel>();
        services.AddTransient<ExchangeResultViewModel>();

        services.AddTransient<ExchangePopupTermsViewModel>();
        services.AddTransient<ExchangePopupIDScanInfoViewModel>();

        return services;
    }

    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddSingleton<IOcrService, OcrService>();
        services.AddSingleton<IOcrProvider, MrzOcrProvider>();
        services.AddSingleton<IOcrProvider, ExternalOcrProvider>();

        services.AddSingleton<ILoggingService, LoggingService>();
        services.AddSingleton<IDataBaseService, DataBaseService>();
        services.AddSingleton<IInitializeService, InitializeService>();

        services.AddSingleton<IAudioService, AudioService>();
        services.AddHttpClient<IApiService, CemsApiService>();

        services.AddSingleton<INavigationService, NavigationService>();
        services.AddSingleton<IPopupService, PopupService>();
        services.AddSingleton<IDialogService, DialogService>();
        services.AddSingleton<IQrGenerateService, QrGenerateService>();
        services.AddSingleton<ILocalizationService>(sp =>
        {
            var logger = sp.GetRequiredService<ILoggingService>();
            var initialCulture = CultureInfo.CurrentUICulture;
            return new LocalizationService(initialCulture, logger);
        });

        return services;
    }

    public static IServiceCollection AddBackgroundServices(this IServiceCollection services)
    {
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

        return services;
    }

    public static IServiceCollection AddStateMachines(this IServiceCollection services)
    {
        services.AddTransient<ExchangeSellStateMachine>();
        services.AddTransient<MockStateMachine>();
        return services;
    }
}
