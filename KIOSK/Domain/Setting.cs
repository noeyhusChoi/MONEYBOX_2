using System;
using System.Collections.Generic;

namespace KIOSK.Domain.Kiosks;

public class Setting
{
    public string DefaultLanguage { get; set; } = string.Empty;

    public Dictionary<string, string> Values { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}
