using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace KIOSK.Services
{
    public interface IPopupService
    {
        bool? ShowDialogAsync<TViewModel>() where TViewModel : class;
        void Close(object viewModel);
    }

    public class PopupService : IPopupService
    {
        private readonly IServiceProvider _provider;
        private readonly Dictionary<object, Window> _openWindows = new();

        public PopupService(IServiceProvider provider)
        {
            _provider = provider;
        }

        public bool? ShowDialogAsync<TViewModel>() where TViewModel : class
        {
            // VM 생성 (DI로 주입)
            var vm = _provider.GetRequiredService<TViewModel>();

            // VM 이름 기반으로 View 찾기 (ex: TermsPopupViewModel → TermsPopupView)
            var viewTypeName = typeof(TViewModel).FullName!.Replace("ViewModel", "View");
            var viewType = Type.GetType(viewTypeName);
            var window = (Window)Activator.CreateInstance(viewType)!;

            window.DataContext = vm;
            _openWindows[vm] = window;

            return Application.Current.Dispatcher.Invoke(() => window.ShowDialog());
        }

        public void Close(object viewModel)
        {
            if (_openWindows.TryGetValue(viewModel, out var window))
            {
                window.Close();
                _openWindows.Remove(viewModel);
            }
        }
    }
}
