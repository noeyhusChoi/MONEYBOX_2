using KIOSK.Bootstrap;
using KIOSK.Services;
using KIOSK.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows;

namespace KIOSK;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private AppBootstrapper _bootstrapper;

    protected override async void OnStartup(StartupEventArgs e)
    {
        System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

#if DEBUG
        Application.Current.Resources["IsDebugVisibility"] = Visibility.Visible;
#else
    Application.Current.Resources["IsDebugVisibility"] = Visibility.Collapsed;
#endif

        base.OnStartup(e);
 
        _bootstrapper = new AppBootstrapper();
        try
        {
            await _bootstrapper.StartAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.ToString(), "Startup error");
            Debug.WriteLine(ex);
            Current.Shutdown();
        }
    }
}