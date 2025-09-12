using KIOSK.DeviceInterface;
using KIOSK.Devices;
using System.Diagnostics;
using System.IO.Ports;
using System.Text;

namespace KIOSK.Managers;

public record DeviceInfo(string DeviceName, string DeviceType, string DeviceComm, string param1, string param2);

public class DeviceManager_Print
{
    private IPrinter printer;

    public DeviceManager_Print()
    {
        try
        {
            printer = new Printer(new Serial("COM10", 19200, 8, 1, Parity.None));
            
            Debug.WriteLine("Device Connect.");
        }
        catch
        {
            Debug.WriteLine("Device Connect Fail.");
        }
    }

    public void cmdPrint(string data)
    {
        string line = "\r\n";
        string dash = new string('=', 46) + line;

        string title = string.Empty;
        title += GetLocalizedValue("Title") + line;
        title += line + line;

        printer.setAlign(1);            // 가운데 정렬
        printer.setStyle(0, 1, 0, 1);   // 크기: 2배, 밑줄 없음, 볼드 없음
        printer.printStr(title);        // 출력

        string info = string.Empty;
        info += GetLocalizedValue("Company") + line;
        info += GetLocalizedValue("Telephone") + line;
        info += GetLocalizedValue("Address") + line;
        info += line + line;

        printer.setAlign(0);            // 왼쪽 정렬
        printer.setStyle(0, 0, 0, 0);   // 초기화
        printer.printStr(info);         // 출력

        string info2 = string.Empty;
        info2 += MakePadLeftString2(GetLocalizedValue("Date"), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        info2 += MakePadLeftString2(GetLocalizedValue("Num"), "0000");

        printer.printStr(info2);
        printer.printStr(dash);

        string current = string.Empty;
        current += MakePadLeftString2(GetLocalizedValue("Currency"), "JPY");
        current += MakePadLeftString2(GetLocalizedValue("ExchangeRate"), "9.239100");
        current += MakePadLeftString2(GetLocalizedValue("AmountPaid"), "10,000 JPY");
        current += MakePadLeftString2(GetLocalizedValue("AmountExchange"), "92,300 KRW");

        printer.printStr(current);
        printer.printStr(dash);
        printer.printStr(line);
        printer.printStr(line);

        printer.cut();

        printer.printStr(data);
    }

    string MakePadLeftString2(string szText1, string szText2)
    {
        int nText1 = Encoding.Default.GetByteCount(szText1);
        int nText2 = Encoding.Default.GetByteCount(szText2);
        int nSpace = 46 - (nText1 + nText2);

        string szSpace = new string(' ', nSpace);

        return string.Format("{0}{1}{2}\r\n", szText1, szSpace, szText2);
    }

    private Dictionary<string, Tuple<string, string>> dict = new Dictionary<string, Tuple<string, string>>()
        {
            { "Title", new Tuple<string, string>("환전영수증", "CURRENCY EXCHANGE RECEIPT") },
            { "Company", new Tuple<string, string>("주식회사 머니박스", "MONEYBOX") },
            { "Telephone", new Tuple<string, string>("TEL:02-0000-0000", "TEL:02-0000-0000") },
            { "Address", new Tuple<string, string>("주소:서울특별시 용산구 한강대로 393", "ADDRESS:393, HANGANG-daero, Yongsan-gu, Seoul") },
            { "Date", new Tuple<string, string>("거래일시", "DATE") },
            { "Num", new Tuple<string, string>("거래번호", "TRANSACTION") },
            { "Currency", new Tuple<string, string>("통화", "CURRENCY") },
            { "ExchangeRate", new Tuple<string, string>("환율", "EXCHANGE RATE") },
            { "AmountPaid", new Tuple<string, string>("지불금액", "AMOUNT PAID") },
            { "AmountExchange", new Tuple<string, string>("환전금액", "AMOUNT EXCHANGE") }
        };

    string GetLocalizedValue(string key, bool isKorean = false)
      => dict.TryGetValue(key, out var t) ? (isKorean ? t.Item1 : t.Item2) : key;

}
