using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KIOSK.Services;
using Localization;

namespace KIOSK.ViewModels.Exchange.Popup
{
    public partial class ExchangePopupTermsViewModel : ObservableObject
    {
        private readonly ILocalizationService _localizationService;
        private readonly IPopupService _popupService;

        [ObservableProperty]
        private Uri source = new Uri("pack://application:,,,/Assets/Image/Terms/Terms_ko-KR.png");

        public ExchangePopupTermsViewModel(ILocalizationService localization, IPopupService popupService)
        {
            _localizationService = localization;    // 언어 판단 추후 LocalizationService 내부에서 파일 세팅 방법으로 전환
            _popupService = popupService;           

            // 언어에 따른 약관 이미지 URI
            source = new Uri("pack://application:,,,/Assets/Image/Terms/Terms_ko-KR.png");
        }

        [RelayCommand]
        private void Close()
        {
            _popupService.Close(this);
        }
    }
}
