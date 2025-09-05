using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KIOSK.Services;
using Localization;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KIOSK.ViewModels.Exchange.Popup
{
    public partial class ExchangePopupIDScanInfoViewModel : ObservableObject
    {
        private readonly ILocalizationService _localizationService;
        private readonly IPopupService _popupService;

        [ObservableProperty]
        private string imgPath;

        [ObservableProperty]
        private string gifPath;

        public ExchangePopupIDScanInfoViewModel(ILocalizationService localization, IPopupService popupService)
        {
            // TODO: 1. 언어에 따른 파일 변환 (1차)
            //       2. 언어 선택시 전환 로직 추가 개발 (2차)

            _localizationService = localization;
            _popupService = popupService;


            Debug.WriteLine(_localizationService.CurrentCulture.TwoLetterISOLanguageName);
            if (_localizationService.CurrentCulture.TwoLetterISOLanguageName == "ko")
            {
                ImgPath = "pack://application:,,,/Assets/Image/IDScan_ID.png";
                gifPath = "pack://application:,,,/Assets/Gif/IDScan_ID.gif";
            }
            else
            {
                ImgPath = "pack://application:,,,/Assets/Image/IDScan_Passport.png";
                gifPath = "pack://application:,,,/Assets/Gif/IDScan_Passport.gif";
            }
        }

        [RelayCommand]
        private void Close()
        {
            _popupService.Close(this);
        }
    }
}
