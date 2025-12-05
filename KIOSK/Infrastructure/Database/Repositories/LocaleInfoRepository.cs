using KIOSK.Infrastructure.Database.DTO;
using KIOSK.Infrastructure.Database.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KIOSK.Infrastructure.Database.Repositories
{
    internal class LocaleInfoRepository : RepositoryBase, IReadRepository<LocaleInfoModel>
    {
        public LocaleInfoRepository(IDatabaseService db) : base(db)
        {
        }

        public Task<IReadOnlyList<LocaleInfoModel>> LoadAllAsync(CancellationToken ct = default)
            => QueryAsync<LocaleInfoModel>("sp_get_locale_info", null, ct);
    }
}
