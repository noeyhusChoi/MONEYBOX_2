using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace KIOSK.ViewModels;

public partial class TestViewModel : ObservableObject
{
    [ObservableProperty]
    private Uri source;

    [ObservableProperty]
    private bool isPlaying;

    public TestViewModel()
    {
        Source = new Uri("Assets/Video/MoneyBoxVideo.mp4", UriKind.Relative); // 경로는 수정 가능
    }

    [RelayCommand]
    private void Play()
    {
        Console.WriteLine("Play");
        IsPlaying = true;
    }

    [RelayCommand]
    private void Pause()
    {
        Console.WriteLine("Pause");
        IsPlaying = false;
    }

    [RelayCommand]
    private void Stop()
    {
        IsPlaying = false;
        Source = null; // 또는 Position 초기화 등 추가
    }
    
}