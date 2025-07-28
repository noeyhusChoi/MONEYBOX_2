namespace KIOSK.DeviceInterface;

public interface IPrinter
{
    /// <summary>
    /// 텍스트 정렬
    /// </summary>
    /// <param name="align"> 0:왼쪽 1:가운데 2:오른쪽 정렬</param>
    public int setAlign(int align);
    
    /// <summary>
    /// 텍스트 스타일
    /// </summary>
    /// <param name="width"> 0:가로확대해제 1:가로확대지정</param>
    /// <param name="height"> 0:세로확대해제 1:세로확대지정</param>
    /// <param name="under">0:밑줄해제 1:밑줄지정</param>
    /// <param name="bold">0:강조해제 1:강조지정</param>
    /// <returns>성공: 0, 실패: -1</returns>
    public int setStyle(int width, int height, int under, int bold);
    
    /// <summary>
    /// 인쇄지 컷팅
    /// </summary>
    /// /// <returns>성공: 0, 실패: -1</returns>
    public int cut();
    
    /// <summary>
    /// 왼쪽 여백 설정
    /// </summary>
    /// <param name="margin"> margin * 0.125mm </param>
    /// <returns>성공: 0, 실패: -1</returns>
    public int LMargin(int left);
    
    /// <summary>
    /// 프린터 상태 확인 패킷 전송
    /// </summary>
    /// <returns>성공: 상태정보 바이트, 실패: -1</returns>
    public int getStatus();
    
    /// <summary>
    /// 텍스트 프린트
    /// </summary>
    /// /// <param name="data">프린트할 데이터</param>
    /// <returns>성공: 0, 실패: -1</returns>
    public int printStr(string data);
    
    /// <summary>
    /// QR 코드 프린트
    /// </summary>
    /// <param name="data">QR 코드에 넣을 데이터 (버전별 지원 사이즈 다름)</param>
    /// <param name="version">QR 코드 버전(1, 3, 5, 9만 지원)</param>
    /// <returns>성공: 0, 실패: -1</returns>
    public int printQrcode(string data, int version);
    
    /// <summary>
    /// 바코드 프린트
    /// </summary>
    /// <param name="data">바코드에 넣을 데이터 (1 < data.length < 256)</param>
    /// <param name="size">바코드 가로 사이즈(3 ~ 9)</param>
    /// <returns>성공: 0, 실패: -1</returns>
    public int printBarcode(string data, int size);
}