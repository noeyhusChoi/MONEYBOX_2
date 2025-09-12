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

        printer.setAlign(1);            // ��� ����
        printer.setStyle(0, 1, 0, 1);   // ũ��: 2��, ���� ����, ���� ����
        printer.printStr(title);        // ���

        string info = string.Empty;
        info += GetLocalizedValue("Company") + line;
        info += GetLocalizedValue("Telephone") + line;
        info += GetLocalizedValue("Address") + line;
        info += line + line;

        printer.setAlign(0);            // ���� ����
        printer.setStyle(0, 0, 0, 0);   // �ʱ�ȭ
        printer.printStr(info);         // ���

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
            { "Title", new Tuple<string, string>("ȯ��������", "CURRENCY EXCHANGE RECEIPT") },
            { "Company", new Tuple<string, string>("�ֽ�ȸ�� �ӴϹڽ�", "MONEYBOX") },
            { "Telephone", new Tuple<string, string>("TEL:02-0000-0000", "TEL:02-0000-0000") },
            { "Address", new Tuple<string, string>("�ּ�:����Ư���� ��걸 �Ѱ���� 393", "ADDRESS:393, HANGANG-daero, Yongsan-gu, Seoul") },
            { "Date", new Tuple<string, string>("�ŷ��Ͻ�", "DATE") },
            { "Num", new Tuple<string, string>("�ŷ���ȣ", "TRANSACTION") },
            { "Currency", new Tuple<string, string>("��ȭ", "CURRENCY") },
            { "ExchangeRate", new Tuple<string, string>("ȯ��", "EXCHANGE RATE") },
            { "AmountPaid", new Tuple<string, string>("���ұݾ�", "AMOUNT PAID") },
            { "AmountExchange", new Tuple<string, string>("ȯ���ݾ�", "AMOUNT EXCHANGE") }
        };

    string GetLocalizedValue(string key, bool isKorean = false)
      => dict.TryGetValue(key, out var t) ? (isKorean ? t.Item1 : t.Item2) : key;

}
