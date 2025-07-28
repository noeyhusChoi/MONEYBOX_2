using System.Windows;
using System.Windows.Controls;

namespace KIOSK.Views;

public partial class TestView : UserControl
{
    public TestView()
    {
        InitializeComponent();
    }

    private void MediaElement_MediaEnded(object sender, RoutedEventArgs e)
    {
        Media.Position = TimeSpan.Zero;
    }
}