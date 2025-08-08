using KIOSK.Bootstrap;
using KIOSK.Services;
using KIOSK.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Configuration;
using System.Data;
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

    protected override void OnStartup(StartupEventArgs e)
    {
        System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

        base.OnStartup(e);
 
        _bootstrapper = new AppBootstrapper();
        _bootstrapper.Start();
    }
}