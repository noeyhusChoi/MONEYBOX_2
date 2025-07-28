using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KIOSK.Services;
using KIOSK.Views;

namespace KIOSK.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly INavigationService _nav;

    [ObservableProperty]
    private object currentViewModel;

    public MainViewModel(INavigationService nav)
    {
        _nav = nav;
        NavigateHome(); // 초기 화면
    }

    [RelayCommand]
    private void NavigateHome() => CurrentViewModel = _nav.GetViewModel<TestViewModel>();
}

