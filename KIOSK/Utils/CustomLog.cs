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
    // TODO: 로깅 시스템 개선 (로그 레벨, 로그 저장, DB 연동, 큐 시스템, 로그 포맷 정의)
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
