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
        Task<bool?> ShowDialogAsync<TViewModel>() where TViewModel : class;
        void Close(object viewModel, bool? dialogResult = true);
    }

    public class PopupService : IPopupService
    {
        private readonly IServiceProvider _provider;
        private readonly ILoggingService _logging;
        private readonly Dictionary<object, Window> _openWindows = new();

        public PopupService(IServiceProvider provider, ILoggingService logging)
        {
            _logging = logging;
            _provider = provider;
        }

        public Task<bool?> ShowDialogAsync<TViewModel>() where TViewModel : class
        {
            // 반드시 UI 스레드에서 동작하도록 Dispatcher.InvokeAsync 사용
            var tcs = new TaskCompletionSource<bool?>();

            Application.Current.Dispatcher.InvokeAsync(() =>
            {
                try
                {
                    // VM 생성 (DI)
                    var vm = _provider.GetRequiredService<TViewModel>();

                    // View 타입 찾기 규칙 (ViewModel -> Window)
                    var viewTypeName = typeof(TViewModel).FullName!.Replace("ViewModel", "View");
                    var viewType = Type.GetType(viewTypeName);
                    if (viewType == null) throw new InvalidOperationException($"View type not found for {typeof(TViewModel).FullName}");

                    var window = (Window)Activator.CreateInstance(viewType)!;
                    window.DataContext = vm;

                    // 소유자 설정 (모달처럼 보이게 하려면 Owner 지정)
                    var owner = Application.Current?.MainWindow;
                    if (owner != null)
                    {
                        window.Owner = owner;
                    }

                    // window 닫힘 이벤트에서 결과 전달
                    void ClosedHandler(object s, EventArgs e)
                    {
                        window.Closed -= ClosedHandler;
                        // DialogResult는 null/true/false 가능
                        tcs.TrySetResult(window.DialogResult);
                        _openWindows.Remove(vm);
                        // owner 재활성화
                        if (owner != null) owner.IsEnabled = true;
                    }

                    // 모달 효과 흉내 : Owner 비활성화(입력 차단)
                    if (owner != null)
                    {
                        owner.IsEnabled = false;
                    }

                    _openWindows[vm] = window;
                    window.Closed += ClosedHandler;

                    // Show — 비모달로 띄움. await 는 tcs.Task에서 처리
                    window.Show();
                    _logging.Info($"Popup Show ({typeof(TViewModel).Name})");
                }
                catch (Exception ex)
                {
                    _logging.Error(ex, "Popup Show Exception");
                    tcs.TrySetException(ex);
                }
            });

            return tcs.Task;
        }

        public void Close(object viewModel, bool? dialogResult = true)
        {
            // UI 스레드에서 닫기
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (_openWindows.TryGetValue(viewModel, out var window))
                {
                    try
                    {
                        // DialogResult를 세팅하면 모달에서는 창이 닫히며 ShowDialog()에 값 으로 전달됨
                        // 여기서는 Show()로 띄웠으므로 DialogResult 설정 후 Close
                        window.DialogResult = dialogResult;
                    }
                    catch(Exception ex)
                    {
                        _logging.Error(ex, "Popup Close Exception");
                        // DialogResult setter는 WindowStyle=None 등에서는 예외 발생할 수 있음.
                    }
                    window.Close();
                    _openWindows.Remove(viewModel);
                }
            });
        }
    }
}

