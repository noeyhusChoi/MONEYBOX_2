using System;
using System.Collections.Generic;
using System.Data;
using KIOSK.Domain.Kiosks;
using KIOSK.Utils;

namespace KIOSK.Dto;

public static class KioskDtoMapper
{
    public static KioskInfoDto MapKioskInfo(DataSet dataSet)
    {
        if (dataSet == null) throw new ArgumentNullException(nameof(dataSet));

        var dto = new KioskInfoDto();

        if (dataSet.Tables.Count > 0 && dataSet.Tables[0].Rows.Count > 0)
        {
            var row = dataSet.Tables[0].Rows[0];
            dto.Kiosk = new KioskDto
            {
                Id = row.GetString("kiosk_id"),
                Pid = row.GetString("kiosk_pid"),
            };
        }

        if (dataSet.Tables.Count > 1 && dataSet.Tables[1].Rows.Count > 0)
        {
            var row = dataSet.Tables[1].Rows[0];
            dto.Shop = new ShopDto
            {
                Name = row.GetString("shop_name"),
                Number = row.GetString("shop_no"),
                Telephone = row.GetString("shop_tel"),
                Owner = row.GetString("shop_owner"),
                Message = row.GetString("shop_message"),
            };
        }

        if (dataSet.Tables.Count > 2)
        {
            foreach (DataRow row in dataSet.Tables[2].Rows)
            {
                var key = row.GetString("key");
                if (string.IsNullOrWhiteSpace(key))
                {
                    continue;
                }

                dto.Settings[key] = row.GetString("value");
            }
        }

        return dto;
    }

    public static IReadOnlyList<DeviceInfoDto> MapDeviceInfo(DataSet dataSet)
    {
        if (dataSet == null) throw new ArgumentNullException(nameof(dataSet));

        var devices = new List<DeviceInfoDto>();
        if (dataSet.Tables.Count == 0)
        {
            return devices;
        }

        foreach (DataRow row in dataSet.Tables[0].Rows)
        {
            devices.Add(new DeviceInfoDto
            {
                Id = row.GetString("device_id"),
                Type = row.GetString("device_type"),
                CommunicationType = row.GetString("comm_type"),
                CommunicationPort = row.GetString("comm_port"),
                CommunicationParameters = row.GetString("comm_params"),
            });
        }

        return devices;
    }

    public static (Kiosk kiosk, Shop shop, Setting setting) ToDomain(this KioskInfoDto dto)
    {
        if (dto == null) throw new ArgumentNullException(nameof(dto));

        var kiosk = new Kiosk
        {
            Id = dto.Kiosk.Id ?? string.Empty,
            Pid = dto.Kiosk.Pid ?? string.Empty,
        };

        var shop = new Shop
        {
            Name = dto.Shop.Name ?? string.Empty,
            Number = dto.Shop.Number ?? string.Empty,
            Telephone = dto.Shop.Telephone ?? string.Empty,
            Owner = dto.Shop.Owner ?? string.Empty,
            Message = dto.Shop.Message ?? string.Empty,
        };

        var setting = new Setting
        {
            DefaultLanguage = dto.Settings.TryGetValue("default_language", out var defaultLanguage)
                ? defaultLanguage ?? string.Empty
                : string.Empty,
            Values = new Dictionary<string, string>(dto.Settings, StringComparer.OrdinalIgnoreCase),
        };

        return (kiosk, shop, setting);
    }

    public static Domain.Kiosks.Device ToDomain(this DeviceInfoDto dto)
    {
        if (dto == null) throw new ArgumentNullException(nameof(dto));

        return new Domain.Kiosks.Device
        {
            Id = dto.Id ?? string.Empty,
            Type = dto.Type ?? string.Empty,
            CommunicationType = dto.CommunicationType ?? string.Empty,
            CommunicationPort = dto.CommunicationPort ?? string.Empty,
            CommunicationParameters = dto.CommunicationParameters ?? string.Empty,
        };
    }
}
