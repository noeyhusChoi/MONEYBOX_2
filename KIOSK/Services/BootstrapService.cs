using KIOSK.Device.Abstractions;
using KIOSK.Devices.Management;
using KIOSK.Infrastructure.Database;
using KIOSK.Infrastructure.Logging;
using KIOSK.Infrastructure.Media;
using KIOSK.Models;
using KIOSK.Services.DataBase;
using KIOSK.Utils;
using Localization;
using Microsoft.Extensions.DependencyInjection;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Windows;
using KIOSK.Infrastructure.Database.DTO;

namespace KIOSK.Services
{
    public interface IBootstrapService
    {
        Task initializeAsync();
    }

    public class BootstrapService : IBootstrapService
    {
        private readonly IDatabaseService _db;
        private readonly ILocalizationService _localization;
        private readonly ILoggingService _logging;
        private readonly ExchangeRateModel _exchangeRateModel;
        private readonly IDeviceManager _deviceManagerV2;
        private readonly IAudioPlayService _audioService;

        private readonly ApiConfigFieldRepository _apiConfigFieldService;
        private readonly DepositFieldRepository _depositService;
        private readonly ReceiptFieldRepository _receiptFieldService;
        private readonly LocaleFieldRepository _localeFieldRepository;
        private readonly WithdrawalCassetteService _withdrawalCassetteService;

        public BootstrapService(IServiceProvider provider)
        {
            _db = provider.GetRequiredService<IDatabaseService>();
            _localization = provider.GetRequiredService<ILocalizationService>();
            _logging = provider.GetRequiredService<ILoggingService>();
            _exchangeRateModel = provider.GetRequiredService<ExchangeRateModel>();
            _audioService = provider.GetRequiredService<IAudioPlayService>();

            _deviceManagerV2 = provider.GetRequiredService<IDeviceManager>();

            // db
            _apiConfigFieldService = provider.GetRequiredService<ApiConfigFieldRepository>();
            _depositService = provider.GetRequiredService<DepositFieldRepository>();
            _receiptFieldService = provider.GetRequiredService<ReceiptFieldRepository>();
            _withdrawalCassetteService = provider.GetRequiredService<WithdrawalCassetteService>();
            _localeFieldRepository = provider.GetRequiredService<LocaleFieldRepository>();
        }

        public async Task initializeAsync()
        {
            await InitializeDatabaseAsync();
            await InitializeLocationAsync();
            await InitializeDeviceAsync();

            Trace.WriteLine(AppDomain.CurrentDomain.BaseDirectory);
            List<string> audioList = new List<string>()
            {
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Sound", "Click.wav"),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Sound", "Bill.wav"),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Sound", "Coin.wav"),
            };

            await _audioService.PreloadAllAsync(audioList);
        }

        private async Task InitializeDatabaseAsync()
        {
            if (!await _db.CanConnectAsync())
            {
                _logging.Warn("Database Can't Connection");

                var x = MessageBox.Show("DB 연결 오류", "종료 확인", MessageBoxButton.OK, MessageBoxImage.Question);
                if (x == MessageBoxResult.OK)
                {
                    Application.Current.Shutdown(); // 프로그램 전체 종료
                }
            }

            try
            {
                #region KIOSK_INFO
                var kioskDs = await _db.QueryAsync<DataSet>("sp_get_kiosk_info", type: CommandType.StoredProcedure);

                if (kioskDs.Tables.Count < 2) return;

                #endregion

                #region DEVICE_INFO
                //var deviceDs = await _db.QueryAsync<DataSet>("sp_get_device_info", type: CommandType.StoredProcedure);

                //if (deviceDs.Tables.Count < 1) return;

                //var deviceDt = deviceDs.Tables[0];
                //if (deviceDt?.Rows.Count > 0)
                //{
                //    foreach (DataRow row in deviceDt.Rows)
                //    {
                //        DeviceModel model = new DeviceModel()
                //        {
                //            Id = row.Get<String>("device_id"),
                //            Type = row.Get<String>("device_type"),
                //            CommType = row.Get<String>("comm_type"),
                //            CommPort = row.Get<String>("comm_port"),
                //            CommParam = row.Get<String>("comm_params")
                //        };

                //        Trace.WriteLine($"Device Model: {model.Id}, {model.Type}, {model.CommType}, {model.CommPort}, {model.CommParam}");
                //        _deviceStore.Devices.Add(model);
                //    }
                //}
                #endregion

                #region API_CONFIG
                await _apiConfigFieldService.InitializeAsync();
                if (_apiConfigFieldService.GetAll().Count < 1)
                {

                }
                #endregion

                #region DEPOSIT_ATTRIBUTE_INFO
                await _depositService.InitializeAsync();
                if (_depositService.GetAllFields().Count < 1)
                {
                    var x = MessageBox.Show("입금 속성 설정 값 오류", "종료 확인", MessageBoxButton.OK, MessageBoxImage.Question);
                }
                #endregion

                #region RECEIPT_FIELD_INFO
                await _receiptFieldService.InitializeAsync();
                if (_receiptFieldService.GetAllFields().Count < 1)
                {
                    var x = MessageBox.Show("영수증 필드 설정 값 오류", "종료 확인", MessageBoxButton.OK, MessageBoxImage.Question);
                }
                #endregion

                #region CASSETTE_INFO
                await _withdrawalCassetteService.InitializeAsync();
                if (_withdrawalCassetteService.Get().Count < 1)
                {
                    var x = MessageBox.Show("출금 카세트 설정 값 오류", "종료 확인", MessageBoxButton.OK, MessageBoxImage.Question);
                }
                #endregion

                #region LOCALE_INFO
                await _localeFieldRepository.InitializeAsync();
                if (_localeFieldRepository.GetAllFields().Count < 1)
                {
                    var x = MessageBox.Show("지역 데이터 값 오류", "종료 확인", MessageBoxButton.OK, MessageBoxImage.Question);
                }
                #endregion

                _logging.Info($"Init Database Successed");
            }
            catch (Exception ex)
            {
                _logging.Error(ex, "Init Database Failed");
            }

        }

        private async Task InitializeLocationAsync()
        {
            try
            {
                // 다국어 서비스 초기화
                LocalizationProvider.Initialize(_localization);

                // 기본 문화권 (시스템/설정에 맞게)
                var current = CultureInfo.CurrentUICulture;
                _localization.SetCulture(current);

                _logging.Info($"Init Localization Successed: {current.Name}");
            }
            catch (Exception ex)
            {
                _logging.Error(ex, "Init Localization Failed");
            }
        }

        private async Task InitializeDeviceAsync()
        {
            //foreach (var device in _deviceStore.Devices)
            //{
            //    //_ = _deviceManager.AddAsync(
            //    //    new DeviceDescriptor(
            //    //        Name: device.Id,
            //    //        Model: device.Type,
            //    //        TransportType: device.CommType,
            //    //        TransportPort: device.CommPort,
            //    //        TransportParam: device.CommParam,
            //    //        ProtocolName: string.Empty
            //    //    ));

            //    _ = _deviceManagerV2.AddAsync(
            //        new DeviceDescriptor(
            //            Name: device.Id,
            //            Model: device.Type,
            //            TransportType: device.CommType,
            //            TransportPort: device.CommPort,
            //            TransportParam: device.CommParam,
            //            ProtocolName: string.Empty
            //        ));
            //}
        }
    }
}
