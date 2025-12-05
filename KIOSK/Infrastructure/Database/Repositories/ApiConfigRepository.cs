using KIOSK.DataBase.DTO;
using KIOSK.Infrastructure.Database.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KIOSK.Infrastructure.Database.DTO;

namespace KIOSK.Infrastructure.Database.Repositories
{
    public  class ApiConfigRepository : RepositoryBase, IReadRepository<ApiConfigModel>
    {
        public ApiConfigRepository(IDatabaseService db) : base(db)
        {

        }

        public Task<IReadOnlyList<ApiConfigModel>> LoadAllAsync(CancellationToken ct = default)
            => QueryAsync<ApiConfigModel>("sp_get_server_info", null, ct);
    }
}
