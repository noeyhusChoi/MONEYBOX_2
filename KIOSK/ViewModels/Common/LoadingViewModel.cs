using CommunityToolkit.Mvvm.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows.Media.Imaging;

namespace KIOSK.ViewModels;

public partial class LoadingViewModel : ObservableObject
{
    [ObservableProperty]
    private BitmapImage gifSource;

    public LoadingViewModel()
    {
        try
        {
            GifSource = GifCache.ProgressGif;//new BitmapImage(new Uri("pack://application:,,,/Assets/Gif/Progress.gif", UriKind.Absolute));
        }
        catch (IOException ex)
        {
            // 파일을 찾지 못했을 때
            Console.WriteLine($"[GIF 경로 오류] {ex.Message}");
        }
        catch (Exception ex)
        {
            // 그 외 예외
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
