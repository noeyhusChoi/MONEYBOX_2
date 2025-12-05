using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KIOSK.Infrastructure.Database.Common;

namespace KIOSK.Infrastructure.Database.DTO
{
    public class DepositCurrencyModel
    {
        [Column("CURRENCY_CODE")]
        public string CurrencyCode{get; set;}
        
        [Column("VALUE")]
        public decimal Denomination { get; set;}

        [Column("ATTRIBUTE_CODE")]
        public string AttributeCode { get; set; }
    }
}
