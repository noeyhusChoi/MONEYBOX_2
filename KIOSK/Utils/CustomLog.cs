using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace KIOSK.Utils
{
    public static class CustomLog
    {

        public static void WriteLine(
        string message = "",
        [CallerFilePath] string file = "",
        [CallerLineNumber] int line = 0,
        [CallerMemberName] string member = "")
        {
            string dt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            Debug.WriteLine($"[{dt}] {Path.GetFileName(file)}:{line} \"{member}\" >> {message}");
        }
    }
}
