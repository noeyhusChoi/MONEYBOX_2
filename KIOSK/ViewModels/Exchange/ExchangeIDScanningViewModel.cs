using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Devices.Core;
using KIOSK.FSM;
using KIOSK.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using WpfApp1.NewFolder;

namespace KIOSK.ViewModels
{
    public partial class ExchangeIDScanningViewModel : ObservableObject, IStepMain, IStepNext, IStepError
    {
        public Func<Task>? OnStepMain { get; set; }
        public Func<bool?, Task>? OnStepNext { get; set; }
        public Action<Exception>? OnStepError { get; set; }

        [ObservableProperty]
        private BitmapImage gifPath;

        private readonly DeviceManager _deviceManager;
        private readonly IOcrService _ocr;

        public ExchangeIDScanningViewModel(DeviceManager deviceManager, IOcrService ocr)
        {
            _deviceManager = deviceManager;
            _ocr = ocr;

            var uri = new Uri("pack://application:,,,/Assets/Gif/Progress.gif", UriKind.Absolute);
            gifPath = LoadBitmapSafe(uri);


            //_deviceManager.SendAsync("")
        }

        [RelayCommand]
        private async Task Loaded(object parameter) // 파라미터 필요없으면 object 대신 없음
        {

        }

        private BitmapImage LoadBitmapSafe(Uri uri)
        {
            var bi = new BitmapImage();
            bi.BeginInit();
            bi.UriSource = uri;
            bi.CacheOption = BitmapCacheOption.OnLoad; // 스트림 닫아도 내부 데이터 유지
            bi.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
            bi.EndInit();
            bi.Freeze(); // 스레드 안전, Freezable 문제 예방
            return bi;
        }

        [RelayCommand]
        private async Task Main()
        {
            
        }

        [RelayCommand]
        private async Task Next(object? o)
        {
            try
            {
#if DEBUG
                OnStepNext?.Invoke(true);
#else
                return;
#endif
            }
            catch (Exception ex)
            {
                OnStepError?.Invoke(ex);
            }
        }
    }
}
