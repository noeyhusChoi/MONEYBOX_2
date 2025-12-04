using KIOSK.Infrastructure.Database;
using KIOSK.Utils;
using System.Data;

namespace KIOSK.Services.DataBase
{
    public readonly record struct LocaleField(string CurrencyCode,
                                              string LanguageCode,
                                              string CountryCode,
                                              string CultureCode,
                                              string LanguageName,
                                              string LanguageNameKo,
                                              string LanguageNameEn,
                                              string CountryNameKo,
                                              string CountryNameEn)
    {
        public Uri FlagPackUri => new Uri($"pack://application:,,,/Assets/FLAG/{CurrencyCode}.png", UriKind.Absolute);
    }

    public sealed class LocaleFieldRepository : BaseField<LocaleField>
    {
        public LocaleFieldRepository(IDatabaseService db) : base(db) { }

        // 사용할 프로시저 이름
        protected override string ProcedureName => "sp_get_locale_info";

        // DataRow -> 매핑
        protected override LocaleField MapRow(DataRow row)
        {
            return new LocaleField()
            {
                CurrencyCode = row.Get<string>("CURRENCY_CODE"),
                LanguageCode = row.Get<string>("LANGUAGE_CODE"),
                CountryCode = row.Get<string>("COUNTRY_CODE"),
                CultureCode = row.Get<string>("CULTURE_CODE"),
                LanguageName = row.Get<string>("LANGUAGE_NAME"),
                LanguageNameKo = row.Get<string>("LANGUAGE_NAME_KO"),
                LanguageNameEn = row.Get<string>("LANGUAGE_NAME_EN"),
                CountryNameKo = row.Get<string>("COUNTRY_NAME_KO"),
                CountryNameEn = row.Get<string>("COUNTRY_NAME_EN"),
            };
        }

        // 전체 필드
        public IReadOnlyList<LocaleField> GetAllFields() => GetAll();
    }
}
