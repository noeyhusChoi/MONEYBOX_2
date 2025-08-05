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
}