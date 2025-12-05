using KIOSK.DataBase.DTO;
using KIOSK.Infrastructure.Database.DTO;
using KIOSK.Infrastructure.Database.Interface;

namespace KIOSK.Infrastructure.Database.Repositories
{
    public class ReceiptRepository : RepositoryBase, IReadRepository<ReceiptModel>
    {
        public ReceiptRepository(IDatabaseService db) : base(db)
        {

        }

        public Task<IReadOnlyList<ReceiptModel>> LoadAllAsync(CancellationToken ct = default)
            => QueryAsync<ReceiptModel>("sp_get_receipt_info", null, ct);
    }
}
