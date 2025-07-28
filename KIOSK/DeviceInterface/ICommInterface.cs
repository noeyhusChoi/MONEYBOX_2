namespace KIOSK.DeviceInterface;

public interface ICommInterface
{
    bool Connect();
    bool Disconnect();
    bool Send(string data);
    bool Send(byte[] data);
    byte[] Receive();
    event EventHandler<string> OnDataReceived;
}