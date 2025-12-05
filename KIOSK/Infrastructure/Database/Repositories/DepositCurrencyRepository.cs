using KIOSK.Infrastructure.Database.DTO;
using KIOSK.Infrastructure.Database.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KIOSK.Infrastructure.Database.Repositories
{
    public  class DepositCurrencyRepository : RepositoryBase, IReadRepository<DepositCurrencyModel>
    {
        public DepositCurrencyRepository(IDatabaseService db) : base(db)
        {

        }

        public Task<IReadOnlyList<DepositCurrencyModel>> LoadAllAsync(CancellationToken ct = default)
            => QueryAsync<DepositCurrencyModel>("sp_get_deposit_attribute_info", null, ct);
    }
}
