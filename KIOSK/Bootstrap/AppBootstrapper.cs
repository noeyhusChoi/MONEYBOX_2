using KIOSK.FSM;
using KIOSK.Managers;
using KIOSK.Models;
using KIOSK.Services;
using KIOSK.ViewModels;
using KIOSK.Views;
using Microsoft.Extensions.DependencyInjection;

namespace KIOSK.Bootstrap;

public class AppBootstrapper
{
    public IServiceProvider ServiceProvider { get; }

    public AppBootstrapper()
    {
        var services = new ServiceCollection();

        //장치 연결
        services.AddSingleton<DeviceManager>();

        // Model 등록
        services.AddSingleton<ExchangeRateModel>();
        
        // ViewModels 등록
        services.AddSingleton<FooterViewModel>();
        services.AddSingleton<MainViewModel>();

        services.AddTransient<HomeViewModel>();
        services.AddTransient<TestViewModel>();
        services.AddSingleton<LoadingViewModel>();          // < Loading >>

        services.AddTransient<Test_CompleteViewModel>();    // TEST
        services.AddTransient<Test_ScanViewModel>();        // TEST
        services.AddTransient<Test_TermsViewModel>();       // TEST
        services.AddTransient<Test_ExchangeRateListViewModel>();       // TEST

        // Services 등록
        services.AddSingleton<INavigationService, NavigationService>();
        services.AddSingleton<Test_StateMachineService>();  // TEST
        services.AddTransient<TestStateMachine>();  // TEST
        services.AddHttpClient<IApiService, CemsApiService>();
        
        ServiceProvider = services.BuildServiceProvider();
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