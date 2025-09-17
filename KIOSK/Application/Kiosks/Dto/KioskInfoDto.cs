using System;
using System.Collections.Generic;

namespace KIOSK.Application.Kiosks.Dto;

public class KioskInfoDto
{
    public KioskDto Kiosk { get; set; } = new();

    public ShopDto Shop { get; set; } = new();

    public Dictionary<string, string> Settings { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}

public class KioskDto
{
    public string? Id { get; set; }

    public string? Pid { get; set; }
}

public class ShopDto
{
    public string? Name { get; set; }

    public string? Number { get; set; }

    public string? Telephone { get; set; }

    public string? Owner { get; set; }

    public string? Message { get; set; }
}
