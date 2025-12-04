using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KIOSK.DataBase.DTO;

namespace KIOSK.Infrastructure.Cache
{
    public static class StaticCache
    {
        // ▼ UI 바인딩 용 (정렬된 리스트)
        public static IReadOnlyList<DeviceModel> DeviceList { get; set; }
            = Array.Empty<DeviceModel>();
        


    }
}
