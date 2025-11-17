using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KIOSK.Services;
using Localization;

namespace KIOSK.ViewModels.Exchange.Popup
{
    public partial class ExchangePopupTermsViewModel : DialogViewModelBase<bool>
    {
        private readonly ILocalizationService _localizationService;

        [ObservableProperty]
        private Uri source = new Uri("pack://application:,,,/Assets/Image/Terms/Terms_ko-KR.png");

        public ExchangePopupTermsViewModel(ILocalizationService localization)
        {
            // TODO: 1. 언어에 따른 파일 변환 (1차)
            //       2. 언어 선택시 전환 로직 추가 개발 (2차)

            _localizationService = localization;    // 언어 판단 추후 LocalizationService 내부에서 파일 세팅 방법으로 전환

            // 언어에 따른 약관 이미지 URI
            Source = new Uri("pack://application:,,,/Assets/Image/Terms/Terms_ko-KR.png");
        }

         [RelayCommand]
        private void Close()
        {
            CloseWithResult(false);
        }

        [RelayCommand]
        public void Accept()
        {
            CloseWithResult(true);
        }

        [RelayCommand]
        public void Cancel()
        {
            CloseWithResult(false);
        }
    }
}
