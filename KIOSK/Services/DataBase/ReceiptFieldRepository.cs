using KIOSK.Infrastructure.Database;
using KIOSK.Utils;
using System.Data;

namespace KIOSK.Services.DataBase
{
    public readonly record struct ReceiptField(string Locale,
                                               string Key,
                                               string Value);

    public sealed class ReceiptFieldRepository : BaseField<ReceiptField>
    {
        public ReceiptFieldRepository(IDatabaseService db) : base(db) { }

        // 사용할 프로시저 이름
        protected override string ProcedureName => "sp_get_receipt_info";

        // DataRow -> 매핑
        protected override ReceiptField MapRow(DataRow row)
        {
            return new ReceiptField()
            {
                Locale = row.Get<string>("INFO_LOCALE"),
                Key = row.Get<string>("INFO_KEY"),
                Value = row.Get<string>("INFO_VALUE")
            };
        }

        // 전체 필드
        public IReadOnlyList<ReceiptField> GetAllFields() => GetAll();


        // 전용 메서드
        private volatile Dictionary<string, Dictionary<string, string>> _lookup
          = new(StringComparer.OrdinalIgnoreCase);

        // 로딩이 끝난 뒤, Dictionary 캐시 구성
        protected override void OnLoaded(IReadOnlyList<ReceiptField> items)
        {
            var next = new Dictionary<string, Dictionary<string, string>>(
                StringComparer.OrdinalIgnoreCase);

            foreach (var f in items)
            {
                if (!next.TryGetValue(f.Locale, out var byKey))
                {
                    byKey = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                    next[f.Locale] = byKey;
                }

                byKey[f.Key] = f.Value; // 중복 시 마지막 값이 이김
            }

            _lookup = next;
        }

        public string? GetValue(string locale, string key)
        {
            var snapshot = _lookup;

            if (snapshot.TryGetValue(locale, out var byKey) &&
                byKey.TryGetValue(key, out var value))
            {
                return value;
            }

            return null;
        }

        // InitializeAsync는 베이스에 이미 있음
        // public Task InitializeAsync(CancellationToken ct = default) => base.InitializeAsync(ct);
    }
}
