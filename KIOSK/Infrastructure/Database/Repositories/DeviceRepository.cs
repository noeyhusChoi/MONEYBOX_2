using KIOSK.DataBase.DTO;
using KIOSK.Infrastructure.Database.DTO;
using KIOSK.Infrastructure.Database.Interface;

namespace KIOSK.Infrastructure.Database.Repositories
{
    public class DeviceRepository : RepositoryBase, IReadRepository<DeviceModel>
    {
        public DeviceRepository(IDatabaseService db) : base(db)
        {

        }

        public Task<IReadOnlyList<DeviceModel>> LoadAllAsync(CancellationToken ct = default)
            => QueryAsync<DeviceModel>("sp_get_device_info", null, ct);
    }
}
