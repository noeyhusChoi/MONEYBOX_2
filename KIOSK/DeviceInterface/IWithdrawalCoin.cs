namespace KIOSK.DeviceInterface;

public interface IWithdrawalCoin
{
    public int reset();
    public int dispense();
    public int getStatus();
}