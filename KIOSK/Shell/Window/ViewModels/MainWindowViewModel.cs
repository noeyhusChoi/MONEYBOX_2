using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KIOSK.Infrastructure.UI.Navigation.Services;
using KIOSK.Shell.Contracts;
using KIOSK.Shell.Sub.Menu.ViewModel;
using KIOSK.Shell.Top.Admin.ViewModels;
using KIOSK.Shell.Top.Main.ViewModels;
using System.Diagnostics;
using KIOSK.Infrastructure.Database.Repositories;
using KIOSK.Shell.Sub.Environment.ViewModel;

namespace KIOSK.ViewModels;

public partial class MainWindowViewModel : ObservableObject, IRootShellHost
{
    private readonly INavigationService _nav;
    private readonly UserShellViewModel _rootShell;   // TopShell 1
    private readonly DeviceRepository _repo;

    [ObservableProperty]
    private object rootViewModel; // MainWindow의 Content

    public MainWindowViewModel(INavigationService nav, UserShellViewModel rootShell, DeviceRepository repo)
    {
        _nav = nav;
        _rootShell = rootShell;

        _nav.AttachRootShell(this);

        _repo = repo;
    }

    public async Task InitializeAsync()
    {
        // 장치 초기화 로직
        //var deviceManager = _provider.GetRequiredService<DeviceManager>();
        //var descriptors = _provider.GetRequiredService<IEnumerable<DeviceDescriptor>>();
        //foreach (var d in descriptors)
        //    await deviceManager.AddAsync(d);

        // TopShell
        await _nav.SwitchTopShell<UserShellViewModel>();
        // SubShell
        await _nav.SwitchSubShell<MenuSubShellViewModel>();
    }

    public void SetTopShell(ITopShellHost shell)
    {
        RootViewModel = shell;
    }

    [RelayCommand]
    private void F0()
    {
    }

    [RelayCommand]
    private void F1()
    {
        Trace.WriteLine($"TOPSHELL      [{_nav.ActiveTopShell}]");
        Trace.WriteLine($"SUBSHELL      [{_nav.ActiveSubShell}]");
        Trace.WriteLine($"VIEW          [{_nav.ActiveFlowView}]");
        Trace.WriteLine($"GLOBAL_POPUP  [{_nav.ActiveTopShell?.PopupContent}] ");
        Trace.WriteLine($"LOCAL_POPUP   [{_nav.ActiveSubShell?.PopupContent}] ");
    }

    [RelayCommand]
    private void F2()
    {
        if (_nav.ActiveTopShell is AdminShellViewModel)
        {
            _nav.SwitchTopShell<UserShellViewModel>();
            _nav.SwitchSubShell<MenuSubShellViewModel>();
        }
        else
        {
            _nav.SwitchTopShell<AdminShellViewModel>();
            _nav.SwitchSubShell<EnvironmentShellViewModel>();
        }

    }

    [RelayCommand]
    private void F3()
    {
        MonitorMover.MoveActiveWindowToNextScreen();
    }


    [RelayCommand]
    private void F4()
    {
        MonitorMover.MoveActiveWindowToNextScreen();
    }

    [RelayCommand]
    private void F5()
    {
        var x = _repo.LoadAllAsync();
        //MonitorMover.MoveActiveWindowToNextScreen();
    }
}


