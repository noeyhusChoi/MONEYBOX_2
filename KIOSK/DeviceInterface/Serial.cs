using System.Diagnostics;
using System.IO.Ports;

namespace KIOSK.DeviceInterface;

public class Serial : ICommInterface
{
    private readonly SerialPort _port;
    public event EventHandler<string> OnDataReceived;
    
    public Serial(string portName, int baudRate, int dataBits, int stopBits, Parity parity)
    {
        _port = new SerialPort(portName, baudRate)
        {
            DataBits = dataBits,
            StopBits = (StopBits)stopBits,
            Parity = parity
        };
        _port.DataReceived += (_, _) => OnDataReceived?.Invoke(this, _port.ReadLine());

        Connect();
    }

    public bool Connect()
    {
        try
        {
            _port.Open();
            Debug.WriteLine("OPEN");
            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("OPEN FAIL");
            return false;
        }
    }

    public bool Disconnect()
    {
        try
        {
            _port.Close();
            return true;
        }
        catch (Exception ex)
        {
            return false;
        }
    }

    public bool Send(string data)
    {
        try
        {
            _port.WriteLine(data);
            return true;
        }
        catch (Exception ex)
        {
            return false;
        }
    }

    public bool Send(byte[] data)
    {
        try
        {
            _port.Write(data, 0, data.Length);
            return true;
        }
        catch (Exception ex)
        {
            return false;
        }
    }
    
    public byte[] Receive()
    {
        _port.ReadTimeout = 500;
        int waited = 0;
        while (_port.BytesToRead == 0 && waited < 500)
        {
            System.Threading.Thread.Sleep(10);
            waited += 10;
        }

        int bytesToRead = _port.BytesToRead;
        if (bytesToRead == 0)
            return null; // 타임아웃 동안 데이터가 없으면 null

        byte[] buffer = new byte[bytesToRead];
            
        try
        {
            _port.Read(buffer, 0, bytesToRead);
            return buffer;
        }
        catch
        {
            return null;
        }
    }
}