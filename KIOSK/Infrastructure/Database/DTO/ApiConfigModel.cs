using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KIOSK.Infrastructure.Database.Common;

namespace KIOSK.Infrastructure.Database.DTO
{
    public readonly record struct ApiConfigField(string ServerName, string ServerUrl, int TimeoutSeconds);

    public class ApiConfigModel
    {
        [Column("SERVER_NAME")]
        public string ServerName { get; set; }

        [Column("SERVER_URL")]
        public string ServerUrl { get; set; }

        [Column("SERVER_KEY")]
        public string ServerKey { get; set; }

        [Column("TIMEOUT_SECONDS")]
        public int TimeoutSeconds { get; set; }
    }
}
