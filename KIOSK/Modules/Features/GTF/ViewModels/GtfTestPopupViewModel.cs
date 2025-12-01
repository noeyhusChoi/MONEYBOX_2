using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using KIOSK.Framework.UI;

namespace KIOSK.Modules.GTF.ViewModels
{
    public partial class GtfTestPopupViewModel : ObservableObject
    {
        private readonly IPopupV2Service _popup;

        public GtfTestPopupViewModel(IPopupV2Service popup)
        {
            _popup = popup;
        }

        [RelayCommand]
        private async Task Close()
        {
            _popup.CloseLocal();
        }


    }
}
