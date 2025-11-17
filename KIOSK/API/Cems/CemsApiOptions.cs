using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KIOSK.API.Cems
{
    public sealed class CemsApiOptions
    {
        public string BaseUrl { get; set; } = "https://cems.moneybox.or.kr";
        public string ApiKey { get; set; } = "C4E7I4W5C4B6L3K4T2C4";
        public int TimeoutSeconds { get; set; } = 15;
    }

    public enum CemsApiCmd { C010, C011, C020, C030, C040, C060, C070 }
}