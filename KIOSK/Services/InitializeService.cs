using KIOSK.Managers;
using KIOSK.Models;
using KIOSK.Stores;
using KIOSK.Utils;
using Localization;
using Microsoft.Extensions.DependencyInjection;
using System.Data;
using System.Globalization;

namespace KIOSK.Services
{
    public interface IInitializeService
    {
        Task initialize();
    }

    public class InitializeService : IInitializeService
    {
        private readonly IDataBaseService _db;
        private readonly ILocalizationService _localization;
        private readonly ExchangeRateModel _exchangeRateModel;
        private readonly DeviceManager _deviceManager;
        private readonly KioskStore _kioskStore;
        private readonly DeviceStore _deviceStore;

        public InitializeService(IServiceProvider provider)
        {
            _db = provider.GetRequiredService<IDataBaseService>();
            _localization = provider.GetRequiredService<ILocalizationService>();
            _exchangeRateModel = provider.GetRequiredService<ExchangeRateModel>();
            _deviceManager = provider.GetRequiredService<DeviceManager>();
            _kioskStore = provider.GetRequiredService<KioskStore>();
            _deviceStore = provider.GetRequiredService<DeviceStore>();
        }

        public async Task initialize()
        {
            await Initialize_Location();
            await Initialize_DataBase();
        }

        private async Task Initialize_DataBase()
        {

            #region KIOSK_INFO
            var kioskDs = await _db.QueryAsync<DataSet>("sp_get_kiosk_info", type: CommandType.StoredProcedure);

            if (kioskDs.Tables.Count < 3) return;

            // kiosk
            var kioskDt = kioskDs.Tables[0];
            if (kioskDt?.Rows.Count > 0)
            {
                var row = kioskDt.Rows[0];
                _kioskStore.KioskInfo.Id = row.Get<String>("kiosk_id", "Defualt");
                _kioskStore.KioskInfo.Pid = row.GetString("kiosk_pid");
            }

            // shop
            var shopDt = kioskDs.Tables[1];
            if (shopDt?.Rows.Count > 0)
            {
                var row = shopDt.Rows[0];
                _kioskStore.ShopInfo.Name = row.GetString("shop_name");
                _kioskStore.ShopInfo.No = row.GetString("shop_no");
                _kioskStore.ShopInfo.Tel = row.GetString("shop_tel");
                _kioskStore.ShopInfo.Owner = row.GetString("shop_owner");
                _kioskStore.ShopInfo.Message = row.GetString("shop_message");
            }

            // settings
            var settingDt = kioskDs.Tables[2];
            if (settingDt?.Rows.Count > 0)
            {
                _kioskStore.SettingInfo.Settings = settingDt
                    .AsEnumerable()
                    .ToDictionary(
                        r => r.GetString("key"),
                        r => r.GetString("value"));
            }
            #endregion

            #region DEVICE_INFO
            var deviceDs = await _db.QueryAsync<DataSet>("sp_get_device_info", type: CommandType.StoredProcedure);

            if (deviceDs.Tables.Count < 1) return;

            var deviceDt = deviceDs.Tables[0];
            if (deviceDt?.Rows.Count > 0)
            {
                foreach (DataRow row in deviceDt.Rows)
                {
                    DeviceModel model = new DeviceModel();
                    model.Id =          row.Get<String>("device_id");
                    model.Type =        row.Get<String>("device_type");
                    model.CommType =    row.Get<String>("comm_type");
                    model.CommPort =    row.Get<String>("comm_port");
                    model.CommParam =   row.Get<String>("comm_params");
                }
            }
        }
                #endregion

        private async Task Initialize_Location()
        {
            // 다국어 서비스 초기화
            LocalizationProvider.Initialize(_localization);

            // 기본 문화권 (시스템/설정에 맞게)
            var current = CultureInfo.CurrentUICulture;
            _localization.SetCulture(current);
        }
    }
}
