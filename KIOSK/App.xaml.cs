using System.Configuration;
using System.Data;
using System.Reflection;
using System.Windows;
using KIOSK.Services;
using KIOSK.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace KIOSK;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    public IServiceProvider ServiceProvider { get; private set; }

    protected override void OnStartup(StartupEventArgs e)
    {
        var services = new ServiceCollection();

        // Services 등록
        services.AddSingleton<INavigationService, NavigationService>();

        // ViewModels 등록
        services.AddSingleton<FooterViewModel>();
        services.AddSingleton<MainViewModel>();
        services.AddTransient<HomeViewModel>();
        services.AddTransient<TestViewModel>();

        var provider = services.BuildServiceProvider();

        var mainWindow = new MainWindow
        {
            DataContext = provider.GetRequiredService<MainViewModel>(),
        };
        mainWindow.Show();
    }
}