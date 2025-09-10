using KIOSK.FSM;
using KIOSK.Managers;
using KIOSK.Models;
using KIOSK.Services;
using KIOSK.Stores;
using KIOSK.ViewModels;
using KIOSK.ViewModels.Exchange.Popup;
using Localization;
using Microsoft.Extensions.DependencyInjection;
using System.Globalization;

namespace KIOSK.Bootstrap;

public class AppBootstrapper
{
    public IServiceProvider ServiceProvider { get; }

    public AppBootstrapper()
    {
        // TODO : DI 컨테이너 증가 시 별도 메서드로 분리 고려

        var services = new ServiceCollection();

        // 장치 연결
        services.AddSingleton<DeviceManager>();

        // Store 등록
        services.AddSingleton<KioskStore>();
        services.AddSingleton<DeviceStore>();

        // Model 등록
        services.AddSingleton<ExchangeRateModel>();

        // ViewModels 등록
        services.AddSingleton<FooterViewModel>();
        services.AddSingleton<MainViewModel>();

        services.AddTransient<ServiceViewModel>();              // 서비스 선택
        services.AddSingleton<LoadingViewModel>();              // 로딩

        services.AddTransient<ExchangeLanguageViewModel>();     // 언어 선택
        services.AddTransient<ExchangeCurrencyViewModel>();     // 통화 선택
        services.AddTransient<ExchangeTermsViewModel>();        // 약관 동의
        services.AddTransient<ExchangeIDScanViewModel>();       // 신분증 스캔 대기
        services.AddTransient<ExchangeIDScanningViewModel>();   // 신분증 스캔 진행
        services.AddTransient<ExchangeIDScanCompleteViewModel>();   // 신분증 스캔 완료
        services.AddTransient<ExchangeDepositViewModel>();      // 입금
        services.AddTransient<ExchangeResultViewModel>();       // 결과

        // ViewModels Popup 등록
        services.AddTransient<ExchangePopupTermsViewModel>();       // 상세 약관 팝업
        services.AddTransient<ExchangePopupIDScanInfoViewModel>();  // 신분증 스캔 상세 팝업

        // Services 등록
        services.AddSingleton<ILoggingService, LoggingService>();               // 로그
        services.AddSingleton<IInitializeService, InitializeService>();         // 화면 전환
        services.AddSingleton<INavigationService, NavigationService>();         // 화면 전환
        services.AddSingleton<IAudioService, AudioService>();                   // 사운드
        services.AddSingleton<IPopupService, PopupService>();                   // 팝업
        //services.AddSingleton<ILocalizationService, LocalizationService>();   // 다국어
        services.AddSingleton<IQrGenerateService, QrGenerateService>();         // QR 코드 생성
        services.AddHttpClient<IApiService, CemsApiService>();                  // 서버 API 통신
        services.AddSingleton<IDataBaseService, DataBaseService>();             // DB
        services.AddSingleton<ILocalizationService>(sp =>
        {
            var logger = sp.GetRequiredService<ILoggingService>();
            var initialCulture = CultureInfo.CurrentUICulture; // 또는 설정에서 읽기
            return new LocalizationService(initialCulture, logger);
        }); // 다국어



        // FSM 등록
        services.AddTransient<ExchangeSellStateMachine>();  // TESTING (데모 버전용)
        services.AddTransient<FSM.MOCK.MockStateMachine>();  // TESTING (데모 버전용)

        // 서비스 빌드
        ServiceProvider = services.BuildServiceProvider();
    }

    public async Task Start()
    {
        var _logging = ServiceProvider.GetRequiredService<ILoggingService>();
        _logging.Info("Starting App");

        var initializeService = ServiceProvider.GetRequiredService<IInitializeService>();
        await initializeService.initialize();

        // MainWindow 생성 및 ViewModel 주입
        var mainWindow = new MainWindow
        {
            DataContext = ServiceProvider.GetRequiredService<MainViewModel>()
        };
        mainWindow.Show();
    }
}