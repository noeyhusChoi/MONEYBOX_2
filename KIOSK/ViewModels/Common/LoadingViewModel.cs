using CommunityToolkit.Mvvm.ComponentModel;
using KIOSK.Services;
using System.Diagnostics;
using System.IO;
using System.Windows.Media.Imaging;

namespace KIOSK.ViewModels;

public partial class LoadingViewModel : ObservableObject
{
    private readonly ILoggingService _logging;

    [ObservableProperty]
    private BitmapImage gifSource;

    public LoadingViewModel(ILoggingService logging)
    {
        try
        {
            GifSource = GifCache.ProgressGif;//new BitmapImage(new Uri("pack://application:,,,/Assets/Gif/Progress.gif", UriKind.Absolute));
        }
        catch (IOException ex)
        {
            // 파일을 찾지 못했을 때
            _logging?.Error(ex, ex.Message);
            Console.WriteLine($"[GIF 경로 오류] {ex.Message}");
        }
        catch (Exception ex)
        {
            // 그 외 예외
            _logging?.Error(ex, ex.Message);
            Console.WriteLine($"[GIF 로딩 예외] {ex.Message}");
        }
    }
}
public static class GifCache
{
    public static readonly BitmapImage ProgressGif;

    static GifCache()
    {
        var img = new BitmapImage();
        img.BeginInit();
        img.UriSource = new Uri("pack://application:,,,/Assets/Gif/Progress.gif", UriKind.Absolute);
        img.CacheOption = BitmapCacheOption.OnLoad;
        img.EndInit();
        ProgressGif = img;
    }
}
