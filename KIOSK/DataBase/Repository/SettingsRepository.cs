using KIOSK.DataBase.Stores;
using KIOSK.Models;
using KIOSK.Services;
using KIOSK.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KIOSK.DataBase.Repository
{
    public interface ISettingsRepository 
    {
        //Task<AppSettings> LoadAsync(CancellationToken ct = default); // 캐시 우선
        //Task<AppSettings> RefreshAsync(CancellationToken ct = default); // DB에서 강제 재로딩

        // 타입별 읽기(편의 메서드)
        Task<KioskModel> GetKioskAsync(CancellationToken ct = default);
        Task<DeviceModel?> GetDeviceConfigAsync(CancellationToken ct = default);
        Task<ShopModel?> GetBranchAsync(CancellationToken ct = default);

        // 타입별 저장
        Task SaveKioskAsync(KioskModel kiosk, CancellationToken ct = default);
        Task SaveDeviceConfigAsync(DeviceModel device, CancellationToken ct = default);
        Task SaveBranchAsync(ShopModel shop, CancellationToken ct = default);
    }

    public class SettingsRepository : ISettingsRepository
    {
        private readonly IDatabaseService _db;
        private readonly ILoggingService _logging;

        public SettingsRepository(IDatabaseService db) 
        {
        }

        // 타입별 읽기(편의 메서드)
        public async Task<KioskModel> GetKioskAsync(CancellationToken ct = default)
        {
            return null;
        }

        public Task<DeviceModel?> GetDeviceConfigAsync(CancellationToken ct = default)
        {
            return null;
        }

        public Task<ShopModel?> GetBranchAsync(CancellationToken ct = default)
        {
            return null;
        }

        // 타입별 저장
        public Task SaveKioskAsync(KioskModel kiosk, CancellationToken ct = default)
        {
            return null;
        }

        public Task SaveDeviceConfigAsync(DeviceModel device, CancellationToken ct = default)
        {
            return null;
        }

        public Task SaveBranchAsync(ShopModel shop, CancellationToken ct = default)
        {
            return null;
        }
    }
}
