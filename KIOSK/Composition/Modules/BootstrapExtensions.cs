using KIOSK.Devices.Management;
using KIOSK.FSM;
using KIOSK.Infrastructure.API.Cems;
using KIOSK.Infrastructure.API.Core;
using KIOSK.Infrastructure.API.Gtf;
using KIOSK.Infrastructure.Cache;
using KIOSK.Infrastructure.Database;
using KIOSK.Infrastructure.Database.Repositories;
using KIOSK.Infrastructure.Logging;
using KIOSK.Infrastructure.Media;
using KIOSK.Infrastructure.Network;
using KIOSK.Infrastructure.Storage;
using KIOSK.Infrastructure.UI.Navigation;
using KIOSK.Infrastructure.UI.Navigation.Services;
using KIOSK.Infrastructure.UI.Navigation.State;
using KIOSK.Models;
using KIOSK.Modules.GTF.ViewModels;
using KIOSK.Services;
using KIOSK.Services.API;
using KIOSK.Services.BackgroundTasks;
using KIOSK.Services.DataBase;
using KIOSK.Services.OCR;
using KIOSK.Services.OCR.Models;
using KIOSK.Services.OCR.Providers;
using KIOSK.Shell.Sub.Environment.ViewModel;
using KIOSK.Shell.Sub.Exchange.ViewModel;
using KIOSK.Shell.Sub.Gtf.ViewModel;
using KIOSK.Shell.Sub.Menu.ViewModel;
using KIOSK.Shell.Top.Admin.ViewModels;
using KIOSK.Shell.Top.Main.ViewModels;
using KIOSK.Utils;
using KIOSK.ViewModels;
using KIOSK.ViewModels.Exchange.Popup;
using Localization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Globalization;
using KIOSK.Infrastructure.UI;
using Pr22;

namespace KIOSK.Composition.Modules
{
    public static class BootstrapExtensions
    {
        public static IServiceCollection AddAppModules(this IServiceCollection services)
        {
            services.AddDeviceInfrastructure();
            services.AddRepositories();
            services.AddServices();
            services.AddViewModels();
            services.AddStateMachines();
            services.AddBackgroundServices();
            return services;
        }

        public static IServiceCollection AddDeviceInfrastructure(this IServiceCollection services)
        {
            services.AddSingleton<IDeviceManager, DeviceManagerV2>();
            services.AddSingleton<IDeviceStatusStore, DeviceStatusStore>();
            services.AddSingleton<IDeviceCommandBus, DeviceCommandBus>();
            services.AddSingleton<IDeviceRuntime, DeviceRuntime>();
            services.AddSingleton<DeviceErrorEventService>();
            services.AddSingleton<ExchangeRateModel>();
            return services;
        }

        public static IServiceCollection AddViewModels(this IServiceCollection services)
        {
            // APP
            services.AddSingleton<MainWindowViewModel>();
            services.AddSingleton<LoadingViewModel>();

            // TOP SHELL
            services.AddSingleton<AdminShellViewModel>();
            services.AddSingleton<UserShellViewModel>();
            services.AddSingleton<FooterViewModel>();

            // SUB SHELL
            services.AddScoped<EnvironmentShellViewModel>();
            services.AddScoped<MenuSubShellViewModel>();
            services.AddScoped<ExchangeShellViewModel>();
            services.AddScoped<GtfSubShellViewModel>();

            // 환경설정
            services.AddScoped<EnvironmentViewModel>();

            // 메뉴
            services.AddScoped<MenuViewModel>();

            // 환전
            services.AddTransient<ExchangeLanguageViewModel>();
            services.AddTransient<ExchangeCurrencyViewModel>();
            services.AddTransient<ExchangeIDScanConsentViewModel>();
            services.AddTransient<ExchangeIDScanGuideViewModel>();
            services.AddTransient<ExchangeIDScanProcessViewModel>();
            services.AddTransient<ExchangeIDScanCompleteViewModel>();
            services.AddTransient<ExchangeDepositViewModel>();
            services.AddTransient<ExchangeWithdrawalViewModel>();
            services.AddTransient<ExchangeResultViewModel>();
            services.AddTransient<ExchangeCompleteViewModel>();
            services.AddTransient<ExchangePopupTermsViewModel>();
            services.AddTransient<ExchangePopupIDScanInfoViewModel>();

            // GTF
            services.AddTransient<GtfLanguageSelectViewModel>();
            services.AddTransient<GtfIdScanConsentViewModel>();
            services.AddTransient<GtfIdScanGuideViewModel>();
            services.AddTransient<GtfIdScanProcessViewModel>();
            services.AddTransient<GtfIdScanCompleteViewModel>();
            services.AddTransient<GtfRefundMethodSelectViewModel>();
            services.AddTransient<GtfCreditGuideViewModel>();
            services.AddTransient<GtfAlipayGuideViewModel>();
            services.AddTransient<GtfWeChatGuideViewModel>();
            services.AddTransient<GtfRefundVoucherRegisterViewModel>();
            services.AddTransient<GtfRefundSignatureViewModel>();
            services.AddTransient<GtfCreditRegisterViewModel>();
            services.AddTransient<GtfAlipayRegisterViewModel>();
            services.AddTransient<GtfWeChatRegisterViewModel>();
            services.AddTransient<GtfAlipayAccountSelectViewModel>();
            services.AddTransient<GtfWeChatRegisterGuideViewModel>();
            services.AddTransient<GtfRefundCompleteViewModel>();
            services.AddTransient<GtfTestPopupViewModel>();

            return services;
        }

        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            // 캐시
            services.AddSingleton<StaticCache>();
            services.AddSingleton<IAppCache, AppCache>();

            // 시스템 기본 서비스
            services.AddSingleton<ILoggingService, LoggingService>();
            services.AddSingleton<IDatabaseService, DatabaseService>();
            services.AddSingleton<IBootstrapService, BootstrapService>();

            // OCR
            services.AddSingleton<DocumentReaderDevice>();
            services.AddSingleton<OcrOptions>();
            services.AddSingleton<MrzOcrProvider>();
            services.AddSingleton<ExternalOcrProvider>();
            services.AddSingleton<IOcrService, OcrService>();

            // DB 서비스
            services.AddSingleton<WithdrawalCassetteService>();

            // 하드웨어/시스템 서비스
            services.AddSingleton<IAudioPlayService, AudioPlayService>();
            services.AddSingleton<IStorageService, StorageService>();
            services.AddSingleton<INetworkService, NetworkService>();

            // API
            services.AddHttpClient<IApiGateway, ApiGateway>((sp, http) =>
            {
                http.Timeout = TimeSpan.FromSeconds(30);
            });
            services.AddScoped<IApiClient, ApiClient>();

            // CEMS
            services.AddOptions<CemsApiOptions>();
            services.AddSingleton<IConfigureOptions<CemsApiOptions>, CemsApiOptionsSetup>();
            services.AddScoped<ICemsApiCmdBuilder, CemsApiCmdBuilder>();
            services.AddScoped<CemsApiService>();

            // GTF
            services.AddOptions<GtfApiOptions>();
            services.AddSingleton<IConfigureOptions<GtfApiOptions>, GtfApiOptionsSetup>();
            services.AddScoped<IGtfApiCmdBuilder, GtfApiCmdBuilder>();
            services.AddScoped<GtfApiService>();

            // Background task handlers (실제 실행은 BackgroundTaskService에서)
            services.AddSingleton<SendCemsTxResultTask>();
            services.AddSingleton<UpdateExchangeRateTask>();

            // UI 서비스
            services.AddSingleton<NavigationState>();
            services.AddSingleton<INavigationService, NavigationService>();
            services.AddSingleton<IPopupService, PopupService>();
            services.AddScoped<IVideoPlayService, VideoPlayService>();
            services.AddSingleton<IQrGenerateService, QrGenerateService>();
            services.AddSingleton<IInactivityService, InactivityService>();

            // 프린트 포맷/출력
            services.AddSingleton<ReceiptPrintService>();

            // 환전 거래
            services.AddSingleton<TransactionModelV2>();
            services.AddSingleton<ITransactionServiceV2, TransactionServiceV2>();

            // GTF
            services.AddSingleton<GtfTaxRefundModel>();
            services.AddSingleton<IGtfTaxRefundService, GtfTaxRefundService>();

            // 다국어
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
                name: "SENT_CEMS_TX_RESULT",
                interval: TimeSpan.FromSeconds(10),
                action: async (sp, ct) =>
                {
                    var handler = sp.GetRequiredService<SendCemsTxResultTask>();
                    await handler.ExecuteAsync(ct);
                }));

            services.AddSingleton(new BackgroundTaskDescriptor(
                name: "UPDATE_EXCHANGE_RATE",
                interval: TimeSpan.FromSeconds(10),
                action: async (sp, ct) =>
                {
                    var handler = sp.GetRequiredService<UpdateExchangeRateTask>();
                    await handler.ExecuteAsync(ct);
                }));

            return services;
        }

        public static IServiceCollection AddStateMachines(this IServiceCollection services)
        {
            services.AddScoped<ExchangeSellStateMachine>();
            services.AddScoped<GtfStateMachine>();
            return services;
        }

        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            services.AddSingleton<ApiConfigRepository>();
            services.AddSingleton<DepositCurrencyRepository>();
            services.AddSingleton<KioskRepository>();
            services.AddSingleton<DeviceRepository>();
            services.AddSingleton<ReceiptRepository>();
            services.AddSingleton<LocaleInfoRepository>();
            return services;
        }
    }
}
