using KIOSK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KIOSK.Stores
{

    public class KioskStore
    {
        public KioskModel KioskInfo { get; set; } = new();
        public SettingModel SettingInfo { get; set; } = new();
        public ShopModel ShopInfo { get; set; } = new();
    }
}
