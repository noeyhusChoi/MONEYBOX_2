using CommunityToolkit.Mvvm.Input;
using KIOSK.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KIOSK.ViewModels.Exchange.Popup
{
    public partial class ConfirmDialogViewModel : DialogViewModelBase<bool>
    {
        public ConfirmDialogViewModel()
        {

        }

        [RelayCommand]
        public void Accept() => CloseWithResult(true);
        [RelayCommand]
        public void Cancel() => CloseWithResult(false);
    }
}
