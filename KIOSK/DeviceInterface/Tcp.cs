using System.Net.Sockets;
using System.Text;

namespace KIOSK.DeviceInterface;

public class Tcp : ICommInterface
{
    private CancellationTokenSource _cts;
    private TcpClient _client;
    private NetworkStream _stream;
    private readonly string _ip;
    private readonly int _port;
    public event EventHandler<string> OnDataReceived;

    public Tcp(string ip, int port)
    {
        _ip = ip;
        _port = port;
    }

    public bool Connect()
    {
        try
        {
            _client = new TcpClient(_ip, _port);
            _stream = _client.GetStream();
            _cts = new CancellationTokenSource();
            Task.Run(() => ReadLoop(_cts.Token));
            
            return true;
        }
        catch (Exception e)
        {
            return false;
        }
    }

    private async Task ReadLoop(CancellationToken token)
    {
        byte[] buffer = new byte[1024];

        try
        {
            while (!token.IsCancellationRequested)
            {
                int count = await _stream.ReadAsync(buffer, 0, buffer.Length, token);
                if (count == 0)
                {
                    // 연결 끊김
                    break;
                }

                string data = Encoding.UTF8.GetString(buffer, 0, count);
                OnDataReceived?.Invoke(this, data);
            }
        }
        catch (OperationCanceledException)
        {
            // 정상 종료
        }
        catch (Exception ex)
        {
            // 로그 또는 오류 처리 필요
            Console.WriteLine($"[TCP ReadLoop Error] {ex.Message}");
        }
    }

    public bool Disconnect()
    {
        try
        {
            _cts?.Cancel(); // Task 중단 요청
            _stream?.Close();
            _client?.Close();
            return true;
        }
        catch (Exception e)
        {
            return false;
        }
    }

    public bool Send(string data)
    {
        try
        {
            _stream.Write(Encoding.UTF8.GetBytes(data));
            return true;
        }
        catch (Exception e)
        {
            return false;
        }
        
    }

    public bool Send(byte[] data)
    {
        try
        {
            _stream.Write(data, 0, data.Length);
            return true;
        }
        catch (Exception e)
        {
            return false;
        }
    }
    
    public byte[] Receive() => throw new NotSupportedException();
}