using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KIOSK.Services;
using KIOSK.ViewModels.Exchange.Popup;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KIOSK.ViewModels
{
    public partial class ExchangeTermsViewModel : ObservableObject, IStepMain, IStepNext, IStepPrevious, IStepError
    {
        public Func<Task>? OnStepMain { get; set; }
        public Func<Task>? OnStepPrevious { get; set; }
        public Func<bool?, Task>? OnStepNext { get; set; }
        public Action<Exception>? OnStepError { get; set; }

        private readonly IPopupService _popupService;

        public ExchangeTermsViewModel(IPopupService popupService)
        {
            _popupService = popupService;
        }

        [RelayCommand]
        private async Task OpenTerms()
        {
            await _popupService.ShowDialogAsync<ExchangePopupTermsViewModel>();
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
