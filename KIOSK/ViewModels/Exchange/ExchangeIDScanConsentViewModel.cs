using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KIOSK.Services;
using KIOSK.ViewModels.Exchange.Popup;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Localization;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

namespace KIOSK.ViewModels
{
    public partial class ExchangeIDScanConsentViewModel : ObservableObject, IStepMain, IStepNext, IStepPrevious, IStepError
    {
        public Func<Task>? OnStepMain { get; set; }
        public Func<Task>? OnStepPrevious { get; set; }
        public Func<bool?, Task>? OnStepNext { get; set; }
        public Action<Exception>? OnStepError { get; set; }

        private readonly ExchangePopupTermsViewModel _popup;
        private readonly IDialogService _dialogService;

        public ExchangeIDScanConsentViewModel(IDialogService dialogService, ExchangePopupTermsViewModel popup)
        {
            _dialogService = dialogService;
            _popup = popup;
        }

        [RelayCommand]
        private async Task OpenTerms()
        {
            await _dialogService.ShowDialogAsync<bool>(_popup);
        }

        [RelayCommand]
        private async Task Main()
        {
            try
            {
                OnStepMain?.Invoke();
            }
            catch (Exception ex)
            {
                OnStepError?.Invoke(ex);
            }
        }

        [RelayCommand]
        private async Task Previous()
        {
            try
            {
                OnStepPrevious?.Invoke();
            }
            catch (Exception ex)
            {
                OnStepError?.Invoke(ex);
            }
        }

        [RelayCommand]
        private async Task Next()
        {
            try
            {
                OnStepNext?.Invoke(true);
            }
            catch (Exception ex)
            {
                OnStepError?.Invoke(ex);
            }
        }

    }
}
