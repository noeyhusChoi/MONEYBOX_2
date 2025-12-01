using KIOSK.Modules.TopShell.Interface;
using KIOSK.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KIOSK.Framework.UI
{
    public interface IPopupV2Service
    {
        // Global Popup
        void ShowGlobal(object viewModel);
        void CloseGlobal();

        // Local Popup
        void ShowLocal(object viewModel);
        void CloseLocal();

        // 전체 정리 (Shell 전환 또는 네비게이션 cleanup)
        void CloseAll();
    }

    public partial class PopupV2Service : IPopupV2Service
    {
        private readonly INavigationService _nav;

        public PopupV2Service(INavigationService nav)
        {
            _nav = nav;
        }

        //  Global Popup
        public void ShowGlobal(object viewModel)
        {
            if (_nav.ActiveTopShell == null)
                return;

            // Local Popup 제거 (우선순위 Global > Local)
            if (_nav.ActiveSubShell is IPopupHost localHost)
            {
                localHost.PopupContent = null;
            }

            _nav.ActiveTopShell.PopupContent = viewModel;
        }

        public void CloseGlobal()
        {
            if (_nav.ActiveTopShell == null)
                return;

            _nav.ActiveTopShell.PopupContent = null;
        }

        //  Local Popup (SubShell 전용)
        public void ShowLocal(object viewModel)
        {
            if (_nav.ActiveSubShell == null)
                return;

            // 이미 Global Popup이 뜨면 Local Popup 금지
            if (_nav.ActiveTopShell?.PopupContent != null)
                return;

            _nav.ActiveSubShell.PopupContent = viewModel;
        }

        public void CloseLocal()
        {
            if (_nav.ActiveSubShell == null)
                return;

            _nav.ActiveSubShell.PopupContent = null;
        }

        // 전체 팝업 제거 (Shell 전환/Flow 교체 시)
        public void CloseAll()
        {
            // Global
            if (_nav.ActiveTopShell != null)
            {
                _nav.ActiveTopShell.PopupContent = null;
            }

            // Local
            if (_nav.ActiveSubShell != null)
            {
                _nav.ActiveSubShell.PopupContent = null;
            }
        }
    }
}
