using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KIOSK.Infrastructure.Database.Common;

namespace KIOSK.Infrastructure.Database.DTO
{
    public class ReceiptModel
    {
        [Column("LOCALE")]
        public string Locale { get; set; }

        [Column("KEY")]
        public string Key { get; set; }

        [Column("VALUE")]
        public string Value { get; set; }
    }
}
