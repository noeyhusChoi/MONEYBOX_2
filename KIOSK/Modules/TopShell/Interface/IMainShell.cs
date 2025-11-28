using KIOSK.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KIOSK.Modules.Shells.Interface
{
    public interface IShellHost : INavigable
    {
        object? CurrentView { get; }

        // SubShell 내부에 FlowView를 셋팅
        void SetInnerView(object view);
    }
}
