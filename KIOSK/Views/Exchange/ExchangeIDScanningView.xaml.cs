using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WpfAnimatedGif;

namespace KIOSK.Views
{
    /// <summary>
    /// ExchangeIDScanningView.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ExchangeIDScanningView : UserControl
    {
        public ExchangeIDScanningView()
        {
            InitializeComponent();

            this.Unloaded += ExchangeIDScanningView_Unloaded;
        }

        private void ExchangeIDScanningView_Unloaded(object? sender, RoutedEventArgs e)
        {
            Debug.WriteLine("ReleaseGif called at " + DateTime.Now);
            try { ImageBehavior.SetAnimatedSource(GifViewer, null); GifViewer.Source = null; Debug.WriteLine("Gif cleared"); }
            catch (Exception ex) { Debug.WriteLine("ReleaseGif failed: " + ex); }
        }
    }
}
