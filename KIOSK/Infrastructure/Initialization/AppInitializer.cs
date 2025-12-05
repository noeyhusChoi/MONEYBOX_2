using KIOSK.Device.Abstractions;
using KIOSK.Devices.Management;
using KIOSK.Infrastructure.Cache;
using KIOSK.Infrastructure.Database;
using KIOSK.Infrastructure.Database.Repositories;
using KIOSK.Infrastructure.Logging;
using KIOSK.Infrastructure.Media;
using KIOSK.Services;
using Localization;
using Microsoft.Extensions.DependencyInjection;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Windows;

namespace KIOSK.Infrastructure.Initialization
{
    public class AppInitializer : IAppInitializer
    {
        private readonly IDatabaseService _db;
        private readonly ILocalizationService _localization;
        private readonly ILoggingService _logging;
        private readonly IAudioPlayService _audioService;
        private readonly IDeviceManager _deviceManager;

        // DB/Cache 서비스들
        private readonly StaticCache _staticCache;
        private readonly ApiConfigRepository _apiConfigRepo;
        private readonly DepositCurrencyRepository _depositCurrencyRepo;
        private readonly KioskRepository _kioskRepo;
        private readonly DeviceRepository _deviceRepo;
        private readonly ReceiptRepository _receiptRepo;
        private readonly LocaleInfoRepository _localeInfoRepo;
        private readonly WithdrawalCassetteService _withdrawalCassetteService;

        public bool IsInitialized { get; private set; }

        public event Action<string>? ProgressChanged;

        public AppInitializer(IServiceProvider sp)
        {
            _db = sp.GetRequiredService<IDatabaseService>();
            _localization = sp.GetRequiredService<ILocalizationService>();
            _logging = sp.GetRequiredService<ILoggingService>();
            _audioService = sp.GetRequiredService<IAudioPlayService>();
            _deviceManager = sp.GetRequiredService<IDeviceManager>();

            _staticCache = sp.GetRequiredService<StaticCache>();
            _apiConfigRepo = sp.GetRequiredService<ApiConfigRepository>();
            _depositCurrencyRepo = sp.GetRequiredService<DepositCurrencyRepository>();
            _kioskRepo = sp.GetRequiredService<KioskRepository>();
            _deviceRepo = sp.GetRequiredService<DeviceRepository>();
            _receiptRepo = sp.GetRequiredService<ReceiptRepository>();
            _localeInfoRepo = sp.GetRequiredService<LocaleInfoRepository>();

            _withdrawalCassetteService = sp.GetRequiredService<WithdrawalCassetteService>();

        }

        public async Task InitializeAsync()
        {
            Update("DB 연결 확인 중...");
            await InitializeDatabaseAsync();

            Update("캐시 구조 로딩 중...");
            await LoadStaticCacheAsync();

            Update("언어/Localization 초기화...");
            InitializeLocalization();

            Update("장치(Device) 초기화...");
            await InitializeDevicesAsync();

            Update("오디오 파일 Preload...");
            await PreloadAudioAsync();

            IsInitialized = true;
            Update("초기화 완료!");
        }

        private void Update(string msg)
        {
            ProgressChanged?.Invoke(msg);
            _logging.Info($"[Init] {msg}");
        }

        private async Task InitializeDatabaseAsync()
        {
            if (!await _db.CanConnectAsync())
                throw new Exception("DB 연결 실패"); // UI에서 처리할 것

            // KIOSK_INFO 불러오기
            await _db.QueryAsync<DataSet>("sp_get_kiosk_info", type: CommandType.StoredProcedure);
        }

        private async Task LoadStaticCacheAsync()
        {
            _staticCache.ApiConfigList = await _apiConfigRepo.LoadAllAsync();
            _staticCache.DepositCurrencyList = await _depositCurrencyRepo.LoadAllAsync();
            _staticCache.Kiosk = await _kioskRepo.LoadAllAsync();
            _staticCache.DeviceList = await _deviceRepo.LoadAllAsync();
            _staticCache.ReceiptList = await _receiptRepo.LoadAllAsync();
            _staticCache.LocaleInfoList = await _localeInfoRepo.LoadAllAsync();

            await _withdrawalCassetteService.InitializeAsync(); // Temporary, Have to Delete
        }

        private void InitializeLocalization()
        {
            LocalizationProvider.Initialize(_localization);
            var culture = CultureInfo.CurrentUICulture;
            _localization.SetCulture(culture);
        }

        private async Task InitializeDevicesAsync()
        {
            foreach (var device in _staticCache.DeviceList)
            {
                await _deviceManager.AddAsync(
                    new DeviceDescriptor(
                        Name: device.Id,
                        Model: device.Type,
                        TransportType: device.CommType,
                        TransportPort: device.CommPort,
                        TransportParam: device.CommParam,
                        ProtocolName: string.Empty
                ));
            }
        }

        private async Task PreloadAudioAsync()
        {
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            List<string> audioList = new()
            {
                Path.Combine(baseDir, "Assets", "Sound", "Click.wav"),
                Path.Combine(baseDir, "Assets", "Sound", "Bill.wav"),
                Path.Combine(baseDir, "Assets", "Sound", "Coin.wav"),
            };

            await _audioService.PreloadAllAsync(audioList);
        }
    }
}
