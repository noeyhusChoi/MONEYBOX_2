using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using KIOSK.Application.Kiosks;
using KIOSK.Application.Kiosks.Dto;
using KIOSK.Domain.Kiosks;
using KIOSK.Services;

namespace KIOSK.Infrastructure.Persistence;

public class KioskRepository : IKioskRepository
{
    private readonly IDataBaseService _database;

    public KioskRepository(IDataBaseService database)
    {
        _database = database;
    }

    public async Task<(Kiosk kiosk, Shop shop, Setting setting)> GetKioskAsync(CancellationToken cancellationToken = default)
    {
        var dataSet = await _database.QueryAsync<DataSet>(
            "sp_get_kiosk_info",
            type: CommandType.StoredProcedure,
            ct: cancellationToken);

        var dto = KioskDtoMapper.MapKioskInfo(dataSet);
        return dto.ToDomain();
    }

    public async Task<IReadOnlyList<Device>> GetDevicesAsync(CancellationToken cancellationToken = default)
    {
        var dataSet = await _database.QueryAsync<DataSet>(
            "sp_get_device_info",
            type: CommandType.StoredProcedure,
            ct: cancellationToken);

        var dtos = KioskDtoMapper.MapDeviceInfo(dataSet);
        return dtos.Select(dto => dto.ToDomain()).ToList();
    }
}
