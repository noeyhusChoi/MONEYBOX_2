using KIOSK.Domain.Kiosks;

namespace KIOSK.Stores;

public class KioskStore
{
    public Kiosk KioskInfo { get; private set; } = new();

    public Setting SettingInfo { get; private set; } = new();

    public Shop ShopInfo { get; private set; } = new();

    public void Update(Kiosk kiosk, Shop shop, Setting setting)
    {
        KioskInfo = kiosk ?? new Kiosk();
        ShopInfo = shop ?? new Shop();
        SettingInfo = setting ?? new Setting();
    }
}
