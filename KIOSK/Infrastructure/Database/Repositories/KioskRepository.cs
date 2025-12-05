using KIOSK.DataBase.DTO;
using KIOSK.Infrastructure.Database.DTO;
using KIOSK.Infrastructure.Database.Interface;

namespace KIOSK.Infrastructure.Database.Repositories
{
    public class KioskRepository : RepositoryBase, IReadRepository<KioskModel>
    {
        public KioskRepository(IDatabaseService db) : base(db)
        { }

        public Task<IReadOnlyList<KioskModel>> LoadAllAsync(CancellationToken ct = default)
            => QueryAsync<KioskModel>("sp_get_kiosk_info", null, ct);
    }
}
