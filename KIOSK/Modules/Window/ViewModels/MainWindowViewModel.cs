using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KIOSK.Services;
using System.Diagnostics;

namespace KIOSK.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    private readonly INavigationService _nav;
    private readonly RootShellViewModel _rootShell;   // TopShell 1

    [ObservableProperty]
    private object rootViewModel; // MainWindow의 Content

    public MainWindowViewModel(INavigationService nav, RootShellViewModel rootShell)
    {
        _nav = nav;
        _rootShell = rootShell;

        // MainWindow의 Root 콘텐츠 = 항상 RootShell
        RootViewModel = _rootShell;

        // NavigationService에 TopShell 등록
    }

    public async Task InitializeAsync()
    {
        // 장치 초기화 로직
        //var deviceManager = _provider.GetRequiredService<DeviceManager>();
        //var descriptors = _provider.GetRequiredService<IEnumerable<DeviceDescriptor>>();
        //foreach (var d in descriptors)
        //    await deviceManager.AddAsync(d);

        await Task.Delay(2000); // 시뮬레이션용 딜레이

        _nav.AttachTopShell(_rootShell);

        // 준비 완료 후: 실제 쉘로 교체
        // 1) MainShell로 진입
        //await _nav.SwitchTopShell<MainShellViewModel>();

        // 2) 첫 화면 = SelectSubShellView
        await _nav.SwitchSubShell<MenuSubShellViewModel>();

        //await _nav.NavigateTo<MenuViewModel>();
    }

    [RelayCommand]
    private void F0()
    {
    }

    [RelayCommand]
    private void F1()
    {
        Trace.WriteLine($"TOPSHELL [{_nav.ActiveTopShell}]");
        Trace.WriteLine($"SUBSHELL [{_nav.ActiveSubShell}]");
        Trace.WriteLine($"VIEW     [{_nav.ActiveFlowView}]");
        Trace.WriteLine($"POPUP    [{_nav.ActiveSubShell.PopupContent}] ");
    }

    [RelayCommand]
    private void F2()
    {
        _nav.NavigateTo<MenuViewModel>();
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
        MonitorMover.MoveActiveWindowToNextScreen();
    }
}


