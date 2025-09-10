using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KIOSK.Models
{
    class LogSettingModel
    {
        public string fileRetainDays { get; set; } = "30";
        public string fileRollingInterval { get; set; } = "1";
        public string fileLimitSize { get; set; } = "10MB";
        public string fileName { get; set; } = "log";
        public string directoryPath { get; set; } = AppContext.BaseDirectory ?? Directory.GetCurrentDirectory();

    }
}
