using System.Collections.Generic;
using KIOSK.Domain.Kiosks;

namespace KIOSK.Stores;

public class DeviceStore
{
    private readonly List<KIOSK.Domain.Kiosks.Device> _devices = new();

    public IReadOnlyList<KIOSK.Domain.Kiosks.Device> Devices => _devices;

    public void SetDevices(IEnumerable<KIOSK.Domain.Kiosks.Device>? devices)
    {
        _devices.Clear();
        if (devices == null)
        {
            return;
        }

        _devices.AddRange(devices);
    }
}
