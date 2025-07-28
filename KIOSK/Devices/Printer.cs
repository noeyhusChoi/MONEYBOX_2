using KIOSK.DeviceInterface;

namespace KIOSK.Devices;

public class Printer : IPrinter
{
    private readonly ICommInterface _comm;
    
    public Printer(ICommInterface comm)
    {
        _comm = comm;
    }
    
    /// <summary>
    /// 텍스트 정렬
    /// </summary>
    /// <param name="align"> 0:왼쪽 1:가운데 2:오른쪽 정렬</param>
    public int setAlign(int align)
    {
        byte value = (byte)align;

        try
        {
            return _comm.Send([0x1b, 0x61, value]) ? 0 : -1;
        }
        catch
        {
            return -1;
        }
    }

    /// <summary>
    /// 텍스트 스타일
    /// </summary>
    /// <param name="width"> 0:가로확대해제 1:가로확대지정</param>
    /// <param name="height"> 0:세로확대해제 1:세로확대지정</param>
    /// <param name="under">0:밑줄해제 1:밑줄지정</param>
    /// <param name="bold">0:강조해제 1:강조지정</param>
    /// <returns>성공: 0, 실패: -1</returns>
    public int setStyle(int width, int height, int under, int bold)
    {
        // global style setting
        byte styleFlags1 = 0;
        if (bold == 1) styleFlags1 |= (1 << 3);      // Bit 3 for bold
        if (height == 1) styleFlags1 |= (1 << 4);    // Bit 4 for height
        if (width == 1) styleFlags1 |= (1 << 5);     // Bit 5 for width
        if (under == 1) styleFlags1 |= (1 << 7);     // Bit 7 for underline

        // korean style setting (한글은 추가 처리 필요)
        byte styleFlags2 = 0;
        if (width == 1) styleFlags2 |= (1 << 2);     // Bit 2 for width
        if (height == 1) styleFlags2 |= (1 << 3);    // Bit 3 for height
        if (under == 1) styleFlags2 |= (1 << 7);     // Bit 7 for underline

        try
        {
            return _comm.Send([0x1b, 0x21, styleFlags1])
                   && _comm.Send([0x1c, 0x21, styleFlags2]) ? 0 : -1;
        }
        catch
        {
            return -1;
        }
    }

    /// <summary>
    /// 인쇄지 컷팅
    /// </summary>
    /// /// <returns>성공: 0, 실패: -1</returns>
    public int cut()
    {
        try
        {
            return _comm.Send([0x1B, 0x69]) ? 0 : -1;
        }
        catch
        {
            return -1;
        }
    }

    /// <summary>
    /// 왼쪽 여백 설정
    /// </summary>
    /// <param name="left"> margin * 0.125mm </param>
    /// <returns>성공: 0, 실패: -1</returns>
    public int LMargin(int left)
    {
        try
        {
            return _comm.Send([0x1d, 0x4c, (byte)left]) ? 0 : -1;
        }
        catch
        {
            return -1;
        }
    }

    /// <summary>
    /// 프린터 상태 확인 패킷 전송
    /// </summary>
    /// <returns>성공: 상태정보 바이트, 실패: -1</returns>
    public int getStatus()
    {
        try
        {
            // 프린터 상태 요청
            _comm.Send(new byte[] { 0x1d, 0x72, 0x01 });
            
            // 프린터 응답 수신
            byte[] response = _comm.Receive();
            
            // 오류
            if (response.Length == 0)
                return -1;
            
            return response[0];
        }
        catch
        {
            return -1;
        }
    }

    /// <summary>
    /// 텍스트 프린트
    /// </summary>
    /// /// <param name="data">프린트할 데이터</param>
    /// <returns>성공: 0, 실패: -1</returns>
    public int printStr(string data)
    {
        byte[] buf = System.Text.Encoding.GetEncoding("ks_c_5601-1987").GetBytes(data);

        try
        {
            return _comm.Send(buf) ? 0 : -1;
        }
        catch
        {
            return -1;
        }
    }

    /// <summary>
    /// QR 코드 프린트
    /// </summary>
    /// <param name="data">QR 코드에 넣을 데이터 (버전별 지원 사이즈 다름)</param>
    /// <param name="version">QR 코드 버전(1, 3, 5, 9만 지원)</param>
    /// <returns>성공: 0, 실패: -1</returns>
    public int printQrcode(string data, int version)
    {
        if (string.IsNullOrEmpty(data))
            return -1;

        // 인코딩 바이트 길이로 체크 (특수문자 포함 대비)
        byte[] buf = System.Text.Encoding.GetEncoding("ks_c_5601-1987").GetBytes(data);

        // 지원하는 버전 및 길이 체크
        int maxLength;
        switch (version)
        {
            case 1: maxLength = 17; break;
            case 3: maxLength = 53; break;
            case 5: maxLength = 106; break;
            case 9: maxLength = 230; break;
            default: return -1; // 미지원 버전
        }

        if (buf.Length > maxLength)
            return -1; // 데이터 길이 초과

        // 커맨드 데이터 조립
        byte mode = 0x02;
        byte dataLength = (byte)(buf.Length & 0xFF);
        byte type = (byte)version;
        byte[] cmd = new byte[] { 0x1A, 0x42, mode, dataLength, type };

        // 전송할 데이터 결합
        byte[] packet = new byte[cmd.Length + buf.Length];
        Buffer.BlockCopy(cmd, 0, packet, 0, cmd.Length);
        Buffer.BlockCopy(buf, 0, packet, cmd.Length, buf.Length);

        try
        {
            return _comm.Send(packet) ? 0 : -1;
        }
        catch (Exception ex)
        {
            return -1;
        }
    }

    /// <summary>
    /// 바코드 프린트
    /// </summary>
    /// <param name="data">바코드에 넣을 데이터 (1 ~ 256)</param>
    /// <param name="size">바코드 가로 사이즈(3 ~ 9)</param>
    /// <returns>성공: 0, 실패: -1</returns>
    public int printBarcode(string data, int size)
    {
        if (string.IsNullOrEmpty(data))
            return -1;

        // 인코딩 바이트 길이로 체크 (특수문자 포함 대비)
        byte[] buf = System.Text.Encoding.GetEncoding("ks_c_5601-1987").GetBytes(data);

        // 커맨드 데이터 조립
        byte mode = 0x01;    // PDF417
        byte dataLength = (byte)(buf.Length & 0xFF);
        byte type = (byte)(size & 0xFF);
        byte[] cmd = new byte[] { 0x1A, 0x42, mode, dataLength, (byte)type, };

        // 전송할 데이터 결합
        byte[] packet = new byte[cmd.Length + buf.Length];
        Buffer.BlockCopy(cmd, 0, packet, 0, cmd.Length);
        Buffer.BlockCopy(buf, 0, packet, cmd.Length, buf.Length);

        try
        {
            return _comm.Send(packet) ? 0 : -1;
        }
        catch (Exception ex)
        {
            return -1;
        }
    }
}