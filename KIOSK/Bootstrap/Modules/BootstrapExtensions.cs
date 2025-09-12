using KIOSK.FSM;
using KIOSK.FSM.MOCK;
using KIOSK.Services;
using KIOSK.ViewModels;
using KIOSK.ViewModels.Exchange.Popup;
using Localization;
using Microsoft.Extensions.DependencyInjection;
using System.Globalization;

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
        services.AddSingleton<ILoggingService, LoggingService>();
        services.AddSingleton<IInitializeService, InitializeService>();
        services.AddSingleton<INavigationService, NavigationService>();
        services.AddSingleton<IAudioService, AudioService>();
        services.AddSingleton<IPopupService, PopupService>();
        services.AddSingleton<IQrGenerateService, QrGenerateService>();
        services.AddHttpClient<IApiService, CemsApiService>();
        services.AddSingleton<IDataBaseService, DataBaseService>();

        services.AddSingleton<ILocalizationService>(sp =>
        {
            var logger = sp.GetRequiredService<ILoggingService>();
            var initialCulture = CultureInfo.CurrentUICulture;
            return new LocalizationService(initialCulture, logger);
        });

        return services;
    }

    public static IServiceCollection AddStateMachines(this IServiceCollection services)
    {
        services.AddTransient<ExchangeSellStateMachine>();
        services.AddTransient<MockStateMachine>();
        return services;
    }
}
