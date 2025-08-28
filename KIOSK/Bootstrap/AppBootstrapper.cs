using KIOSK.FSM;
using KIOSK.Managers;
using KIOSK.Models;
using KIOSK.Services;
using KIOSK.ViewModels;
using KIOSK.ViewModels.Exchange.Popup;
using KIOSK.Views;
using Localization;
using Microsoft.Extensions.DependencyInjection;
using System.Globalization;

namespace KIOSK.Bootstrap;

public class AppBootstrapper
{
    public IServiceProvider ServiceProvider { get; }

    public AppBootstrapper()
    {
        var services = new ServiceCollection();

        // 장치 연결
        services.AddSingleton<DeviceManager>();

        // Model 등록
        services.AddSingleton<ExchangeRateModel>();
        
        // ViewModels 등록
        services.AddSingleton<FooterViewModel>();
        services.AddSingleton<MainViewModel>();

        services.AddTransient<ServiceViewModel>();          // 서비스 선택
        services.AddSingleton<LoadingViewModel>();          // 로딩

        services.AddTransient<ExchangeLanguageViewModel>(); // 언어 선택
        services.AddTransient<ExchangeTermsViewModel>();    // TODO: TESTING
        services.AddTransient<ExchangeCurrencyViewModel>(); // 통화 선택
        services.AddTransient<ExchangePopupTermsViewModel>();// 상세 약관 팝업

        services.AddTransient<Test_CompleteViewModel>();    // TEST
        services.AddTransient<Test_ScanViewModel>();        // TEST
        services.AddTransient<Test_TermsViewModel>();       // TEST

        // Services 등록
        services.AddSingleton<INavigationService, NavigationService>();         // 화면 전환
        services.AddSingleton<IPopupService, PopupService>();                   // 팝업
        services.AddSingleton<ILocalizationService, LocalizationService>();     // 다국어
        services.AddSingleton<IQrGenerateService, QrGenerateService>();         // QR 코드 생성
        services.AddHttpClient<IApiService, CemsApiService>();                  // 서버 API 통신

        // FSM 등록
        services.AddTransient<TestStateMachine>();  // TEST

        // 서비스 빌드
        ServiceProvider = services.BuildServiceProvider();

        LocalizationProvider.Initialize(ServiceProvider.GetRequiredService<ILocalizationService>());

        // 기본 문화권 (시스템/설정에 맞게)
        var current = CultureInfo.CurrentUICulture;
        ServiceProvider.GetRequiredService<ILocalizationService>().SetCulture(current);
    }

    public void Start()
    {
        // MainWindow 생성 및 ViewModel 주입
        var mainWindow = new MainWindow
        {
            DataContext = ServiceProvider.GetRequiredService<MainViewModel>()
        };
        mainWindow.Show();
    }
}