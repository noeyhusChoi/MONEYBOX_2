using System.IO.Ports;
using KIOSK.DeviceInterface;
using KIOSK.Devices;

namespace KIOSK.Managers;

public record DeviceInfo(string DeviceName, string DeviceType, string DeviceComm, string param1, string param2);

public class DeviceManager
{
    private IPrinter printer;
    
    public DeviceManager()
    {
        printer = new Printer(new Serial("COM3", 9600, 8, 1, Parity.None));
    }
    
    public void cmdPrint(string data)
    {
        printer.printStr(data);
    }
}
