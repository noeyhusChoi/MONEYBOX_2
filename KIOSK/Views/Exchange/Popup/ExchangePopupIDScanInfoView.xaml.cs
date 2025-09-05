using System;
using System.Collections.Generic;
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

namespace KIOSK.Views.Exchange.Popup
{
    /// <summary>
    /// ExchangePopupIDScanInfoView.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ExchangePopupIDScanInfoView : Window
    {
        public ExchangePopupIDScanInfoView()
        {
            InitializeComponent();
        }

        private void GifImage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var img = (Image)sender;
            img.Clip = new RectangleGeometry(
                new Rect(0, 0, e.NewSize.Width, e.NewSize.Height),
                20, 20); // RadiusX, RadiusY
        }
    }
}
