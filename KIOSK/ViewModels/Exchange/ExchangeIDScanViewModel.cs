using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KIOSK.Services;
using KIOSK.ViewModels.Exchange.Popup;

namespace KIOSK.ViewModels
{
    public partial class ExchangeIDScanViewModel : ObservableObject, IStepMain, IStepNext, IStepPrevious, IStepError
    {
        private readonly IPopupService _popup;

        public Func<Task>? OnStepMain { get; set; }
        public Func<Task>? OnStepPrevious { get; set; }
        public Func<bool?, Task>? OnStepNext { get; set; }
        public Action<Exception>? OnStepError { get; set; }

        public ExchangeIDScanViewModel(IPopupService popup)
        {
            _popup = popup;
        }

        [RelayCommand]
        private async Task Loaded(object parameter) // 파라미터 필요없으면 object 대신 없음
        {
            // 렌더링이 끝나길 잠깐 대기 (선택적, 팝업을 띄울 때 유용)
            //await Task.Yield();

            // 안전하게 비동기 팝업 호출
            await _popup.ShowDialogAsync<ExchangePopupIDScanInfoViewModel>();
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
