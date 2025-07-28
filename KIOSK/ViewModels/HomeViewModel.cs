using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace KIOSK.ViewModels;

public partial class HomeViewModel : ObservableObject
{
    [ObservableProperty]
    private string _welcomeMessage = "환영합니다! 홈 화면입니다.";

    [RelayCommand]
    private void RefreshMessage()
    {
        WelcomeMessage = $"현재 시간: {DateTime.Now:T}";
    }
}