using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;

namespace KIOSK.ViewModels;

public partial class FooterViewModel : ObservableObject
{
    [ObservableProperty]
    private BaseViewModel currentViewModel;

    [ObservableProperty]
    private string branchName = "서울역점";

    [ObservableProperty]
    private string date = DateTime.Now.ToString("yyyy.MM.dd. (ddd)").ToUpper();

    [ObservableProperty]
    private string time;

    private readonly DispatcherTimer timer;

    public FooterViewModel()
    {
        timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(0.5)
        };
        timer.Tick += (s, e) => Time = DateTime.Now.ToString("HH:mm:ss");
        timer.Start();
    }
}