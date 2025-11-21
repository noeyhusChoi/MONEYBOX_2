using CommunityToolkit.Mvvm.ComponentModel;
using KIOSK.Views.Exchange.Popup;
using System.Windows;

namespace KIOSK.Services
{
    public class DialogViewModelBase<TResult> : ObservableObject
    {
        /// <summary>
        /// ViewModel이 팝업을 닫을 때 결과를 전달하기 위한 이벤트
        /// </summary>
        public event EventHandler<TResult?>? RequestClose;

        protected void CloseWithResult(TResult? result)
            => RequestClose?.Invoke(this, result);

        /// <summary>
        /// 호출자(호스트)에서 팝업을 닫고 결과를 전달할 때 사용.
        /// UI 스레드가 아닌 곳에서 호출해도 안전하게 UI 스레드로 마샬링합니다.
        /// </summary>
        public void RequestCloseFromCaller(TResult? result = default)
        {
            // Application.Current이 null일 가능성은 거의 없지만 안전하게 검사
            var dispatcher = Application.Current?.Dispatcher;
            if (dispatcher != null && !dispatcher.CheckAccess())
            {
                // UI 스레드로 마샬링하여 이벤트 발생
                dispatcher.Invoke(() => CloseWithResult(result));
            }
            else
            {
                CloseWithResult(result);
            }
        }
    }

    public interface IDialogService
    {
        Task<TResult?> ShowDialogAsync<TResult>(DialogViewModelBase<TResult> vm);
    }

    public partial class DialogService : IDialogService
    {
        public Task<TResult?> ShowDialogAsync<TResult>(DialogViewModelBase<TResult> vm)
        {
            if (vm == null) throw new ArgumentNullException(nameof(vm));

            var tcs = new TaskCompletionSource<TResult?>();

            // Ensure on UI thread
            Application.Current.Dispatcher.Invoke(() =>
            {
                // PopupWindow는 아래에 제공된 XAML을 사용
                var wnd = new PopupWindow
                {
                    Owner = Application.Current.MainWindow,
                    DataContext = vm,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                    // optional: SizeToContent = SizeToContent.WidthAndHeight
                };

                // Disable owner to emulate modal, but non-blocking.
                var owner = wnd.Owner;
                if (owner != null)
                {
                    owner.IsEnabled = false;
                }

                // 1) ViewModel에서 Close 요청 처리
                void OnRequestClose(object? s, TResult? result)
                {
                    vm.RequestClose -= OnRequestClose;
                    // set result and close window
                    tcs.TrySetResult(result);
                    // close window (triggers Closed)
                    if (wnd.IsLoaded)
                        wnd.Close();
                    else
                    {
                        // If not loaded yet, schedule close after loaded
                        wnd.Loaded += (_, __) => wnd.Close();
                    }
                }
                vm.RequestClose += OnRequestClose;

                // 2) If user closes window via UI (X 버튼 등) -> complete with default(null)
                void OnClosed(object? s, EventArgs e)
                {
                    wnd.Closed -= OnClosed;
                    vm.RequestClose -= OnRequestClose;
                    // Re-enable owner
                    if (owner != null)
                        owner.IsEnabled = true;

                    // If tcs not completed by ViewModel, complete with default(null)
                    tcs.TrySetResult(default);
                }
                wnd.Closed += OnClosed;

                // Show non-modal window
                wnd.Show();
            });

            return tcs.Task;
        }
    }
}
