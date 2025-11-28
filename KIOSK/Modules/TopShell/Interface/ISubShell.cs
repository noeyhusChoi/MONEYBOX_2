using KIOSK.ViewModels;

namespace KIOSK.Modules.Shells.Interface
{
    public interface ITopShellHost : INavigable
    {
        // TopShell 내부에서 현재 어떤 SubShell이 활성인지
        object? CurrentSubShell { get; }

        // TopShell 내부에 SubShell을 붙인다
        void SetSubShell(object? shell);
    }

    public interface ISubShellPopupHost
    {
        void CloseLocalPopup();
    }
}
