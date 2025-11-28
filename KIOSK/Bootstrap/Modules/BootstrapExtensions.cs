using KIOSK.API.Cems;
using KIOSK.API.Core;
using KIOSK.API.GTF;
using KIOSK.FSM;
using KIOSK.Models;
using KIOSK.Modules.GTF.ViewModels;
using KIOSK.Modules.OCR.Models;
using KIOSK.Services;
using KIOSK.Services.API;
using KIOSK.Services.DataBase;
using KIOSK.Utils;
using KIOSK.ViewModels;
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
using WpfApp1.NewFolder;

namespace KIOSK.Bootstrap.Modules;

public static class BootstrapExtensions
{
    public static IServiceCollection AddViewModels(this IServiceCollection services)
    {
        // APP
        services.AddSingleton<MainWindowViewModel>();
        
        // 공용
        services.AddSingleton<LoadingViewModel>();

        // 관리자 TOP SHELL
        services.AddSingleton<EnvironmentViewModel>();

        // 사용자 TOP SHELL
        services.AddSingleton<RootShellViewModel>(); 
        services.AddSingleton<FooterViewModel>();

        // 사용자/메뉴 SHELL
        services.AddSingleton<MenuShellViewModel>();

        // 서비스 선택
        services.AddTransient<ServiceViewModel>();

        // 여기까지 우선 테스트




        // 환전
        services.AddScoped<ExchangeLanguageViewModel>();
        services.AddScoped<ExchangeCurrencyViewModel>();
        services.AddScoped<ExchangeIDScanConsentViewModel>();
        services.AddScoped<ExchangeIDScanGuideViewModel>();
        services.AddScoped<ExchangeIDScanProcessViewModel>();
        services.AddScoped<ExchangeIDScanCompleteViewModel>();
        services.AddScoped<ExchangeDepositViewModel>();
        services.AddScoped<ExchangeWithdrawalViewModel>();
        services.AddScoped<ExchangeResultViewModel>();
        services.AddScoped<ExchangeCompleteViewModel>();

        services.AddScoped<ExchangePopupTermsViewModel>();
        services.AddScoped<ExchangePopupIDScanInfoViewModel>();

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

        return services;
    }

    public static IServiceCollection AddServices(this IServiceCollection services)
    {
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
        services.AddSingleton<ApiConfigFieldService>();
        services.AddSingleton<DepositFieldService>();
        services.AddSingleton<ReceiptFieldService>();
        services.AddSingleton<WithdrawalCassetteService>();
        services.AddSingleton<LocaleFieldService>();

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
            var apiConfig = sp.GetRequiredService<ApiConfigFieldService>();
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
            var apiConfig = sp.GetRequiredService<ApiConfigFieldService>();
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
        services.AddSingleton<INavigationService, NavigationService>();
        services.AddSingleton<IPopupService, PopupService>();
        services.AddSingleton<IDialogService, DialogService>();
        services.AddSingleton<IInactivityService, InactivityService>(); // 유휴 시간 감지
        services.AddScoped<IVideoPlayService, VideoPlayService>();   // 영상 재생 플레이어

        // 기타 서비스
        services.AddSingleton<IQrGenerateService, QrGenerateService>();

        // 프린트 포맷 및 출력
        services.AddSingleton<ReceiptPrintService>();

        // 환전 거래 기록
        services.AddSingleton<TransactionModelV2>();
        services.AddSingleton<ITransactionServiceV2, TransactionServiceV2>();

        // GTF
        services.AddSingleton<GtfTaxRefundModel>();
        services.AddSingleton<IGtfTaxRefundService, GtfTaxRefundService>();

        // 동작 감지
        services.AddSingleton<IInactivityService, InactivityService>();

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
        services.AddTransient<ExchangeSellStateMachine>();
        services.AddTransient<GtfStateMachine>();
        return services;
    }
}
