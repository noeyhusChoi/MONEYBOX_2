using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KIOSK.Models
{
    // 키오스크
    public class KioskModel
    {
        public string Id { get; set; } = string.Empty;
        public string Pid { get; set; } = string.Empty;
    }

    public class SettingModel
    {
        public string DefaultLanguage { get; set; } = string.Empty;
        public Dictionary<string, string> Settings { get; set; } = new();
    }

    public class ShopModel
    {
        public string Name { get; set; } = string.Empty;
        public string No { get; set; } = string.Empty;
        public string Tel { get; set; } = string.Empty;
        public string Owner { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }

    // 장비
    public class DeviceModel
    {
        public string Id;
        public string Type;
        public string CommType;
        public string CommPort;
        public string CommParam;
    }

    //관리자

    public class AdminModel
    {
        public string Id;
        public string Password;
    }

    public class AdminHistoryModel
    {
        public string Id;
        public string Action;

    }


    // TODO: 화폐 부분은 추후 수정
    public class CurrencyModel
    {
        public string Code;
        public string Symbol;
        public int DecimalPlace;
    }

    public class DepositModel 
    { 
        public Dictionary<string ,HashSet<decimal>> Denomination;
    }
}
