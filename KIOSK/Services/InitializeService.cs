using Devices.Abstractions;
using Devices.Core;
using KIOSK.Infrastructure.Persistence;
using KIOSK.Models;
using KIOSK.Stores;
using Localization;
using Microsoft.Extensions.DependencyInjection;
using System.Globalization;
using System.Windows;

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
        private readonly ILoggingService _logging;
        private readonly ExchangeRateModel _exchangeRateModel;
        private readonly DeviceManager _deviceManager;
        private readonly KioskStore _kioskStore;
        private readonly DeviceStore _deviceStore;
        private readonly IKioskRepository _kioskRepository;

        public InitializeService(IServiceProvider provider)
        {
            _db = provider.GetRequiredService<IDataBaseService>();
            _localization = provider.GetRequiredService<ILocalizationService>();
            _logging = provider.GetRequiredService<ILoggingService>();
            _exchangeRateModel = provider.GetRequiredService<ExchangeRateModel>();
            _deviceManager = provider.GetRequiredService<DeviceManager>();
            _kioskStore = provider.GetRequiredService<KioskStore>();
            _deviceStore = provider.GetRequiredService<DeviceStore>();
            _kioskRepository = provider.GetRequiredService<IKioskRepository>();
        }

        public async Task initialize()
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
            await Initialize_DataBase();
            await Initialize_Location();
            await Initialize_Device();
        }

        private async Task Initialize_DataBase()
        {
            try
            {
                var (kiosk, shop, setting) = await _kioskRepository.GetKioskAsync();
                _kioskStore.Update(kiosk, shop, setting);

                var devices = await _kioskRepository.GetDevicesAsync();
                _deviceStore.SetDevices(devices);

                _logging.Info($"Init Database Successed");
            }
            catch (Exception ex)
            {
                _logging.Error(ex, "Init Database Failed");
            }
        }

        private Task Initialize_Location()
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

            return Task.CompletedTask;
        }

        private Task Initialize_Device()
        {
            foreach (var device in _deviceStore.Devices)
            {
                _ = _deviceManager.AddAsync(
                    new DeviceDescriptor(
                        device.Id,
                        device.Type,
                        device.CommunicationType,
                        device.CommunicationPort,
                        device.CommunicationParameters,
                        "",
                        3000,
                        true
                    ));
            }

            return Task.CompletedTask;
        }
    }
}
