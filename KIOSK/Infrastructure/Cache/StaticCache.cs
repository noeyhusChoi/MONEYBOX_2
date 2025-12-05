using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KIOSK.DataBase.DTO;
using KIOSK.Infrastructure.Database.DTO;

namespace KIOSK.Infrastructure.Cache
{
    public class StaticCache
    {
        // ▼ UI 바인딩 용 (정렬된 리스트)
        public IReadOnlyList<KioskModel> Kiosk { get; set; } = Array.Empty<KioskModel>();
        public IReadOnlyList<DeviceModel> DeviceList { get; set; } = Array.Empty<DeviceModel>();
        public IReadOnlyList<LocaleInfoModel> LocaleInfoList { get; set; } = Array.Empty<LocaleInfoModel>();
        public IReadOnlyList<ReceiptModel> ReceiptList { get; set; } = Array.Empty<ReceiptModel>();
        public IReadOnlyList<ApiConfigModel> ApiConfigList { get; set; } = Array.Empty<ApiConfigModel>();
        public IReadOnlyList<DepositCurrencyModel> DepositCurrencyList { get; set; } = Array.Empty<DepositCurrencyModel>();
    }
}
