using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using KIOSK.Domain.Kiosks;

namespace KIOSK.Application.Kiosks;

public interface IKioskRepository
{
    Task<(Kiosk kiosk, Shop shop, Setting setting)> GetKioskAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Device>> GetDevicesAsync(CancellationToken cancellationToken = default);
}
