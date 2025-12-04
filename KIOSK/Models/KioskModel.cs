using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KIOSK.Models
{
    // 키오스크

    public class SettingModel
    {
        public string DefaultLanguage { get; set; } = string.Empty;
        public Dictionary<string, string> Settings { get; set; } = new();
    }

    public class ShopModel
    {
        public string Name { get; set; } = string.Empty;
        public string No { get; set; } = string.Empty;
        public string Tel { get; set; } = string.Empty;
        public string Owner { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }


}
