using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KIOSK.Services;
using KIOSK.Views;
using Microsoft.Extensions.DependencyInjection;

namespace KIOSK.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly IServiceProvider _provider;

    private object _currentViewModel;
    public object CurrentViewModel
    {
        get => _currentViewModel;
        set
        {
            // ���� ViewModel Dispose
            if (_currentViewModel is IDisposable disposable)
                disposable.Dispose();

            SetProperty(ref _currentViewModel, null); // 1. �ϴ� null �Ҵ� (VisualTree���� ������ ���� ����)
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            SetProperty(ref _currentViewModel, value); // 2. ���� �Ҵ�
        }
    }

    [ObservableProperty]
    private object footerViewModel;

    public Action<object>? NavigateAction { get; set; }
    
    public MainViewModel(FooterViewModel footerViewModel, IServiceProvider provider)
    {
        _provider = provider;

        CurrentViewModel = _provider.GetRequiredService<TestViewModel>();
        FooterViewModel = _provider.GetRequiredService<FooterViewModel>();
        
        NavigateAction = vm => CurrentViewModel = vm;
    }
    
    [RelayCommand]
    private void NavigateToHome()
    {
        var _nav = _provider.GetRequiredService<INavigationService>();
        _nav.NavigateTo<TestViewModel>();
    }
}

