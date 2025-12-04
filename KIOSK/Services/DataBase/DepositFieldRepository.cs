using KIOSK.Infrastructure.Database;
using KIOSK.Services.DataBase;
using KIOSK.Utils;
using System.Data;

namespace KIOSK.Services
{
    public readonly record struct DepositField(string CurrencyCode, decimal Denomination, string AttributeCode);

    public sealed class DepositFieldRepository : BaseField<DepositField>
    {
        public DepositFieldRepository(IDatabaseService db) : base(db) { }

        // 사용할 프로시저 이름
        protected override string ProcedureName => "sp_get_deposit_attribute_info";

        // DataRow -> 매핑
        protected override DepositField MapRow(DataRow row)
        {
            return new DepositField()
            {
                CurrencyCode = row.Get<string>("CURRENCY_CODE"),
                Denomination = row.Get<decimal>("VALUE"),
                AttributeCode = row.Get<string>("ATTRIBUTE_CODE")
            };
        }

        // 전체 필드
        public IReadOnlyList<DepositField> GetAllFields() => GetAll();
    }
}
