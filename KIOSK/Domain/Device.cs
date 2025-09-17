namespace KIOSK.Domain.Kiosks;

public class Device
{
    public string Id { get; set; } = string.Empty;

    public string Type { get; set; } = string.Empty;

    public string CommunicationType { get; set; } = string.Empty;

    public string CommunicationPort { get; set; } = string.Empty;

    public string CommunicationParameters { get; set; } = string.Empty;
}
