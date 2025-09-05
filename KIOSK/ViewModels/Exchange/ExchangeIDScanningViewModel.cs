using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KIOSK.FSM;
using KIOSK.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KIOSK.ViewModels
{
    public partial class ExchangeIDScanningViewModel : ObservableObject, IStepMain, IStepNext, IStepError
    {
        public Func<Task>? OnStepMain { get; set; }
        public Func<bool?, Task>? OnStepNext { get; set; }
        public Action<Exception>? OnStepError { get; set; }

        [ObservableProperty]
        private string gifPath = "pack://application:,,,/Assets/Gif/Progress.gif";

        public ExchangeIDScanningViewModel()
        {

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
