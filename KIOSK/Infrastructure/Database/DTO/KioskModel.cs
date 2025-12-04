using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KIOSK.Infrastructure.Database.Common;

namespace KIOSK.Infrastructure.Database.DTO
{
    public class KioskModel
    {
        [Column("KIOSK_ID")]
        public string Id { get; set; }

        [Column("KIOSK_PID")]
        public string Pid { get; set; }
    }
}
