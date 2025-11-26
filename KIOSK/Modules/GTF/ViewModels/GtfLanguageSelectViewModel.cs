using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KIOSK.API.GTF.KIOSK.API.Gtf;
using KIOSK.Services;
using KIOSK.Services.API;
using KIOSK.Services.DataBase;
using KIOSK.ViewModels;
using Localization;
using System.Collections.ObjectModel;
using System.Globalization;

namespace KIOSK.Modules.GTF.ViewModels
{
    public partial class GtfLanguageSelectViewModel : ObservableObject, IStepMain, IStepNext, IStepPrevious, IStepError, INavigable
    {

        [ObservableProperty]
        private ObservableCollection<LocaleField> localeField;

        private readonly ILocalizationService _localizationService;
        private readonly LocaleFieldService _localeFieldService;
        private readonly GtfApiService _gtfApiService;
        private readonly IGtfTaxRefundService _gtfTaxRefundService;

        public Func<Task>? OnStepMain { get; set; }
        public Func<Task>? OnStepPrevious { get; set; }
        public Func<string?, Task>? OnStepNext { get; set; }
        public Action<Exception>? OnStepError { get; set; }

        public GtfLanguageSelectViewModel(ILocalizationService localizationService, LocaleFieldService localeFieldService, GtfApiService gtfApiService, IGtfTaxRefundService gtfTaxRefundService)
        {
            _localizationService = localizationService;
            _localeFieldService = localeFieldService;
            _gtfApiService = gtfApiService;
            _gtfTaxRefundService = gtfTaxRefundService;

            _gtfTaxRefundService.Reset();   // 모델 초기화
            var usedLanguage = new[]
            {
                "ZH-CN",
                "ZH-TW",
                "EN-GB",
                "JA-JP",
                "FR-FR",
                "ES-ES",
                "TH-TH",
                "MS-MY",
                "ID-ID",
                "RU-RU",
                "AR-SA",
                "KO-KR"
            };

            LocaleField = new ObservableCollection<LocaleField>(_localeFieldService.GetAllFields()
                                                                                   .Where(f => usedLanguage.Contains(f.CultureCode))
                                                                                   .OrderBy(f => Array.IndexOf(usedLanguage, f.CultureCode)));
        }

        public Task OnLoadAsync(object? parameter, CancellationToken ct)
        {
            _ = Task.Run(() => InitAsync(ct), ct);
            return Task.CompletedTask;
        }

        public async Task OnUnloadAsync()
        {
            // TODO: 언로드 시 필요한 작업 수행
        }

        private async Task InitAsync(CancellationToken ct)
        {
            InitialRequestDto req = new InitialRequestDto()
            {
                Edi = "01",
                TmlId = "A1",
                ShopName = "테스트1"
            };

            var res = await _gtfApiService.InitialAsync(req, ct);
            
            _gtfTaxRefundService.ApplyInitialResponse(req, res);
        }

        #region Commands
        [RelayCommand]
        private async Task Main()
        {
            try
            {
                if (OnStepMain is not null)
                    await OnStepMain();
            }
            catch (Exception ex)
            {
                if (OnStepError is not null)
                    OnStepError(ex);
            }
        }

        [RelayCommand]
        private async Task Previous()
        {
            try
            {
                if (OnStepPrevious is not null)
                    await OnStepPrevious();
            }
            catch (Exception ex)
            {
                if (OnStepError is not null)
                    OnStepError(ex);
            }
        }


        [RelayCommand]
        private async Task Next(object? parameter)
        {
            if (parameter is string selectedLanguage)
            {
                try
                {
                    var culture = new CultureInfo(selectedLanguage);

                    _localizationService.SetCulture(culture);

                    if (OnStepNext is not null)
                        await OnStepNext(selectedLanguage);
                }
                catch (Exception ex)
                {
                    OnStepError?.Invoke(ex);
                }
            }
        }
        #endregion
    }
}
