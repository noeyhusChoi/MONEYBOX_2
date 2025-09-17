using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using KIOSK.Domain.Kiosks;

namespace KIOSK.Infrastructure.Persistence;

public interface IKioskRepository
{
    Task<(Kiosk kiosk, Shop shop, Setting setting)> GetKioskAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Domain.Kiosks.Device>> GetDevicesAsync(CancellationToken cancellationToken = default);
}
