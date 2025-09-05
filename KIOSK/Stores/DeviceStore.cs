using KIOSK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KIOSK.Stores
{
    class DeviceStore
    {
        public List<DeviceModel> Devices { get; set; } = new();
    }
}
