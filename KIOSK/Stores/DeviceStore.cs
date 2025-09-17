using System.Collections.Generic;
using KIOSK.Domain.Kiosks;

namespace KIOSK.Stores;

public class DeviceStore
{
    private readonly List<Device> _devices = new();

    public IReadOnlyList<Device> Devices => _devices;

    public void SetDevices(IEnumerable<Device>? devices)
    {
        _devices.Clear();
        if (devices == null)
        {
            return;
        }

        _devices.AddRange(devices);
    }
}
