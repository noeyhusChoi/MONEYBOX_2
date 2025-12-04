using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KIOSK.Infrastructure.Database.Common;

namespace KIOSK.DataBase.DTO
{
    // 장비
    public class DeviceModel
    {
        [Column("DEVICE_ID")]
        public string Id { get; set; }

        [Column("DEVICE_TYPE")]
        public string Type { get; set; }

        [Column("COMM_TYPE")]
        public string CommType { get; set; }

        [Column("COMM_PORT")]
        public string CommPort { get; set; }

        [Column("COMM_PARAMS")]
        public string CommParam { get; set; }
    }
}
