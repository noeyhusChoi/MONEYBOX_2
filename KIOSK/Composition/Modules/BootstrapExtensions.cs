using KIOSK.Infrastructure.API.Cems;
using KIOSK.Infrastructure.Media;
using KIOSK.Infrastructure.Logging;
using KIOSK.Infrastructure.API.Core;
using KIOSK.Infrastructure.API.Gtf;
using KIOSK.Infrastructure.UI;
using KIOSK.Infrastructure.UI.Navigation;
using KIOSK.FSM;
using KIOSK.Models;
using KIOSK.Modules.GTF.ViewModels;
using KIOSK.Services.OCR.Models;
using KIOSK.Services;
using KIOSK.Services.API;
using KIOSK.Services.DataBase;
using KIOSK.Infrastructure.Database;
using KIOSK.Shell.Top.Admin.ViewModels;

using KIOSK.Utils;
using KIOSK.ViewModels;
using KIOSK.Shell.Sub.Menu.ViewModel;
using KIOSK.Shell.Sub.Gtf.ViewModel;
using KIOSK.Shell.Sub.Exchange.ViewModel;
using KIOSK.Shell.Top.Main.ViewModels;
using KIOSK.ViewModels.Exchange.Popup;
using Localization;
using Microsoft.Extensions.DependencyInjection;
using MySqlConnector;
using Pr22;
using System.Collections.ObjectModel;
using System.Data;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using KIOSK.Infrastructure.Cache;
using KIOSK.Infrastructure.Database.Repositories;
using KIOSK.Infrastructure.UI.Navigation.Services;
using KIOSK.Infrastructure.UI.Navigation.State;
using KIOSK.Infrastructure.Network;
using KIOSK.Infrastructure.Storage;
using KIOSK.Shell.Sub.Gtf.ViewModel;
using KIOSK.Services.OCR;
using KIOSK.Services.OCR.Providers;
using KIOSK.Shell.Sub.Environment.ViewModel;

namespace KIOSK.Composition.Modules;

public static class BootstrapExtensions
{
    public static IServiceCollection AddViewModels(this IServiceCollection services)
    {
        // APP
        services.AddSingleton<MainWindowViewModel>();

        // 공용
        services.AddSingleton<LoadingViewModel>();

        // TOP SHELL
        services.AddSingleton<AdminShellViewModel>();   // 관리자
        services.AddSingleton<UserShellViewModel>();    // 사용자
        services.AddSingleton<FooterViewModel>();       // 사용자 푸터

        // SUB SHELL
        services.AddScoped<EnvironmentShellViewModel>();     // 관리자/환경설정
        services.AddScoped<MenuSubShellViewModel>();    // 사용자/메뉴
        services.AddScoped<ExchangeShellViewModel>();   // 사용자/환전
        services.AddScoped<GtfSubShellViewModel>();     // 사용자/세금 환급 (GTF)

        // 환경설정 뷰
        services.AddScoped<EnvironmentViewModel>();     // 관리자/환경설정

        // 메뉴 뷰
        services.AddScoped<MenuViewModel>();

        // 환전 뷰
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

        // GTF 뷰
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
        // 시스템 캐시
        services.AddSingleton<StaticCache>();    // 캐시

        // 시스템 기본 서비스
        services.AddSingleton<ILoggingService, LoggingService>();       // 로깅
        services.AddSingleton<IDatabaseService, DatabaseService>();     // DB 접속
        services.AddSingleton<IBootstrapService, BootstrapService>();   // 초기화

        // OCR
        services.AddSingleton<DocumentReaderDevice>();
        services.AddSingleton<OcrOptions>();
        services.AddSingleton<MrzOcrProvider>();
        services.AddSingleton<ExternalOcrProvider>();
        services.AddSingleton<IOcrService, OcrService>();

        // DB 서비스
        services.AddSingleton<ApiConfigFieldRepository>();
        services.AddSingleton<DepositFieldRepository>();
        services.AddSingleton<ReceiptFieldRepository>();
        services.AddSingleton<WithdrawalCassetteService>();
        services.AddSingleton<LocaleFieldRepository>();

        // DB 레포지토리
        services.AddSingleton<DeviceRepository>();


        // 하드웨어,시스템 서비스
        services.AddSingleton<IAudioPlayService, AudioPlayService>();
        services.AddSingleton<IStorageService, StorageService>();
        services.AddSingleton<INetworkService, NetworkService>();

        // API
        services.AddHttpClient<IApiGateway, ApiGateway>((sp, http) =>
        {
            http.Timeout = TimeSpan.FromSeconds(30);    // 옵션, 없어도 옵션에서 처리 가능
        });
        services.AddScoped<IApiClient, ApiClient>();

        // CEMS
        services.AddSingleton<CemsApiOptions>(sp =>
        {
            var apiConfig = sp.GetRequiredService<ApiConfigFieldRepository>();
            //apiConfig.InitializeAsync().GetAwaiter().GetResult();

            var config = apiConfig.GetRequired("CEMS");   // SERVER_NAME='CEMS'

            return new CemsApiOptions
            {
                BaseUrl = config.ServerUrl,
                TimeoutSeconds = config.TimeoutSeconds
            };
        });
        services.AddScoped<ICemsApiCmdBuilder, CemsApiCmdBuilder>();
        services.AddScoped<CemsApiService>();

        // GTF
        services.AddSingleton<GtfApiOptions>(sp =>
        {
            var apiConfig = sp.GetRequiredService<ApiConfigFieldRepository>();
            //apiConfig.InitializeAsync().GetAwaiter().GetResult();

            var config = apiConfig.GetRequired("GTF");     // SERVER_NAME='GTF'

            return new GtfApiOptions
            {
                BaseUrl = config.ServerUrl,
                TimeoutSeconds = config.TimeoutSeconds
            };
        });
        services.AddScoped<IGtfApiCmdBuilder, GtfApiCmdBuilder>();
        services.AddScoped<GtfApiService>();

        // UI 서비스
        services.AddSingleton<NavigationState>();
        services.AddSingleton<INavigationService, NavigationService>();
        services.AddSingleton<IPopupService, PopupService>();
        services.AddScoped<IVideoPlayService, VideoPlayService>();   // 영상 재생 플레이어
        services.AddSingleton<IQrGenerateService, QrGenerateService>();
        services.AddSingleton<IInactivityService, InactivityService>();

        // 프린트 포맷 및 출력
        services.AddSingleton<ReceiptPrintService>();

        // 환전 거래 기록
        services.AddSingleton<TransactionModelV2>();
        services.AddSingleton<ITransactionServiceV2, TransactionServiceV2>();

        // GTF
        services.AddSingleton<GtfTaxRefundModel>();
        services.AddSingleton<IGtfTaxRefundService, GtfTaxRefundService>();

        // 다국어 지원
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
                // sp는 scope.ServiceProvider (DB 등 안전 사용)
                var logger = sp.GetRequiredService<ILoggingService>();

                // outbox 목록
                var db = sp.GetRequiredService<IDatabaseService>();
                var dt = await db.QueryAsync<DataTable>(@"sp_get_tx_outbox", type: CommandType.StoredProcedure);

                if (dt.Rows.Count > 0)
                {
                    var cemsApiService = sp.GetRequiredService<CemsApiService>();

                    foreach (DataRow row in dt.Rows)
                    {
                        // json to transactionmodel
                        var transaction = JsonConvertExtension.ConvertFromJson<TransactionModelV2>(row["PAYLOAD_JSON"]?.ToString() ?? string.Empty);

                        var res = await cemsApiService.RegisterTransactionAsync(transaction, ct);

                        if (res.Result && res.ECode == null)
                        {
                            await db.QueryAsync<DataTable>(@"sp_update_tx_outbox_success",
                            new[]
                            {
                                DatabaseService.Param("@tx_id", MySqlDbType.VarChar, transaction.TransactionID)
                            },
                            type: CommandType.StoredProcedure);
                        }
                        else
                        {
                            await db.QueryAsync<DataTable>(@"sp_update_tx_outbox_fail",
                            new[]
                            {
                                DatabaseService.Param("@tx_id", MySqlDbType.VarChar, transaction.TransactionID)
                            },
                            type: CommandType.StoredProcedure);
                        }
                    }
                }

                await Task.CompletedTask;
            }));

        services.AddSingleton(new BackgroundTaskDescriptor(
            name: "UPDATE_EXCHANGE_RATE",
            interval: TimeSpan.FromSeconds(10),
            action: async (sp, ct) =>
            {
                // sp는 scope.ServiceProvider (DB 등 안전 사용)
                var logger = sp.GetRequiredService<ILoggingService>();

                var cems = sp.GetRequiredService<CemsApiService>();
                var result = await cems.GetRateAllAsync(ct);

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    NumberHandling = JsonNumberHandling.AllowReadingFromString
                };

                var model = sp.GetRequiredService<ExchangeRateModel>();

                // 3) CEMS 응답에서 data JSON 꺼내서 바인딩
                if (!result.Fields.TryGetValue("data", out var dataJson) || string.IsNullOrWhiteSpace(dataJson))
                {
                    // data가 없으면 빈 모델로 정리 (필요에 따라 예외 던져도 됨)
                    model.Result = false;
                    model.Data = new ObservableCollection<ExchangeRate>();
                }
                else
                {
                    var list = JsonSerializer.Deserialize<ObservableCollection<ExchangeRate>>(dataJson, options)
                               ?? new ObservableCollection<ExchangeRate>();

                    // CEMS 전체 결과 성공 여부는 CemsApiResponse.Result에서 받는 게 자연스러움
                    model.Result = result.Result;
                    model.Data = list;
                }

                // 4) 후처리 - 단위 보정
                if (model.Result && model.Data != null)
                {
                    const decimal scale = 0.01m;

                    foreach (var data in model.Data)
                    {
                        switch (data.Currency)
                        {
                            case "VND":
                            case "JPY":
                            case "IDR":
                                data.Base *= scale;
                                data.Sell *= scale;
                                data.Buy *= scale;
                                data.SpSell *= scale;
                                data.SpBuy *= scale;
                                break;
                            default:
                                break;
                        }
                    }
                }

                await Task.CompletedTask;
            }));

        return services;
    }

    public static IServiceCollection AddStateMachines(this IServiceCollection services)
    {
        services.AddScoped<ExchangeSellStateMachine>();
        services.AddScoped<GtfStateMachine>();
        return services;
    }
}
