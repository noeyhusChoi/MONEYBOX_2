using KIOSK.Modules.TopShell.Interface;
using KIOSK.ViewModels;

namespace KIOSK.Modules.Shells.Interface
{
    public interface ISubShellHost : INavigable, IPopupHost
    {
        object? CurrentView { get; }

        // SubShell 내부에 FlowView를 셋팅
        void SetInnerView(object view);
    }
}
