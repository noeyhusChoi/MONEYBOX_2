using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using WpfAnimatedGif;

namespace KIOSK.Services
{
    public interface IPopupService
    {
        Task<bool?> ShowDialogAsync<TViewModel>(object? ownerViewModel = null) where TViewModel : class;
        Task<(Task<bool?> ResultTask, TViewModel ViewModel)> ShowDialogWithHandleAsync<TViewModel>(object? ownerViewModel = null)
            where TViewModel : class;
        void Close(object viewModel, bool? dialogResult = true);
        void CloseAllDebug();
    }

    public class PopupService : IPopupService
    {
        private readonly IServiceProvider _provider;
        private readonly ILoggingService _logging;

        // key: popup VM instance, value: (window, scope)
        private readonly Dictionary<object, (Window Window, IServiceScope? Scope)> _openWindows = new();

        public PopupService(IServiceProvider provider, ILoggingService logging)
        {
            _provider = provider;
            _logging = logging;
        }

        public Task<bool?> ShowDialogAsync<TViewModel>(object? ownerViewModel = null) where TViewModel : class
            => ShowDialogWithHandleAsync<TViewModel>(ownerViewModel).ContinueWith(t => t.Result.ResultTask).Unwrap();

        public async Task<(Task<bool?> ResultTask, TViewModel ViewModel)> ShowDialogWithHandleAsync<TViewModel>(object? ownerViewModel = null)
            where TViewModel : class
        {
            var tcs = new TaskCompletionSource<bool?>(TaskCreationOptions.RunContinuationsAsynchronously);

            var vm = await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                // 1) scope + VM 생성 (등록 없어도 ActivatorUtilities로 주입됨)
                var scope = _provider.CreateScope();
                var sp = scope.ServiceProvider;
                var vmLocal = ActivatorUtilities.CreateInstance<TViewModel>(sp);

                // 2) View 찾기
                var viewTypeName = typeof(TViewModel).FullName!.Replace("ViewModel", "View");
                var viewType = Type.GetType(viewTypeName)
                              ?? throw new InvalidOperationException($"View type not found for {typeof(TViewModel).FullName}");
                var window = (Window)Activator.CreateInstance(viewType)!;
                window.DataContext = vmLocal;

                // 3) Owner 결정
                Window? owner = null;
                if (ownerViewModel != null)
                {
                    owner = Application.Current?.Windows
                              .OfType<Window>()
                              .FirstOrDefault(w => ReferenceEquals(w.DataContext, ownerViewModel)
                                                || (w.DataContext != null && w.DataContext.Equals(ownerViewModel)));
                }
                owner ??= Application.Current?.MainWindow;

                if (owner != null)
                {
                    window.Owner = owner;
                    owner.IsEnabled = false;
                }

                // 4) Closed 핸들러 — 정리 순서 보장
                void ClosedHandler(object? s, EventArgs e)
                {
                    try
                    {
                        // Close()에서 DialogResult를 세팅하지 않으므로 Tag에 담긴 값을 우선 사용
                        bool? result = window.DialogResult;
                        if (!result.HasValue && window.Tag is bool b) result = b;
                        if (!result.HasValue && window.Tag is bool nb) result = nb;

                        tcs.TrySetResult(result);
                    }
                    catch (Exception ex)
                    {
                        _logging.Error(ex, "ClosedHandler: set result failed");
                    }
                    finally
                    {
                        try { ReleaseAllImagesInWindow(window); } catch { /* ignore */ }

                        try
                        {
                            window.Content = null;
                            window.DataContext = null;
                            window.Tag = null;
                        }
                        catch { /* ignore */ }

                        // 딕셔너리 제거는 scope dispose 이후/이전 상관없지만, 여기선 먼저 제거
                        _openWindows.Remove(vmLocal);

                        // **중요: scope는 클로저로 캡처한 걸 직접 Dispose (딕셔너리 재조회 X)**
                        try { scope.Dispose(); } catch { /* ignore */ }

                        try { if (owner != null) owner.IsEnabled = true; } catch { /* ignore */ }

                        window.Closed -= ClosedHandler;

#if DEBUG
                        LogMemory("After popup closed");
#endif
                    }
                }

                _openWindows[vmLocal] = (window, scope);
                window.Closed += ClosedHandler;

                window.Show();
                _logging.Info($"Popup shown: {typeof(TViewModel).Name} (Owner: {owner?.GetType().Name ?? "none"})");

#if DEBUG
                LogMemory("After popup shown");
#endif
                return vmLocal;
            });

            return (tcs.Task, vm);
        }

        // 외부에서 닫기: DialogResult는 건드리지 말고 Tag에 결과만 남긴 뒤 Close()
        public void Close(object viewModel, bool? dialogResult = true)
        {
            if (viewModel == null) return;

            Application.Current.Dispatcher.Invoke(() =>
            {
                if (_openWindows.TryGetValue(viewModel, out var info))
                {
                    try
                    {
                        info.Window.Tag = dialogResult; // 결과 전달용
                        info.Window.Close();            // ClosedHandler에서 모든 정리/결과 처리
                    }
                    catch (Exception ex)
                    {
                        _logging.Error(ex, "Close (exact key) failed");
                    }
                    return;
                }

                // ReferenceEquals fallback
                var pair = _openWindows.FirstOrDefault(kv => ReferenceEquals(kv.Key, viewModel));
                if (!Equals(pair, default(KeyValuePair<object, (Window, IServiceScope?)>)))
                {
                    try
                    {
                        pair.Value.Window.Tag = dialogResult;
                        pair.Value.Window.Close();
                    }
                    catch (Exception ex)
                    {
                        _logging.Error(ex, "Close (reference fallback) failed");
                    }
                    return;
                }

                _logging.Warn($"Close: no matching popup for vm={viewModel.GetType().FullName}");
            });
        }

        public void CloseAllDebug()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                foreach (var kv in _openWindows.ToArray())
                {
                    try
                    {
                        kv.Value.Window.Tag = false;
                        kv.Value.Window.Close();
                        kv.Value.Scope?.Dispose();
                    }
                    catch { }
                }
                _openWindows.Clear();

                GC.Collect();
                GC.WaitForPendingFinalizers();
                LogMemory("After CloseAllDebug");
            });
        }

        private static void ReleaseAllImagesInWindow(Window window)
        {
            var root = window.Content as DependencyObject;
            if (root == null) return;

            var q = new Queue<DependencyObject>();
            q.Enqueue(root);
            while (q.Count > 0)
            {
                var cur = q.Dequeue();
                int n = VisualTreeHelper.GetChildrenCount(cur);
                for (int i = 0; i < n; i++)
                {
                    var child = VisualTreeHelper.GetChild(cur, i);
                    if (child is Image img)
                    {
                        try
                        {
                            ImageBehavior.SetAnimatedSource(img, null);
                            img.Source = null;
                        }
                        catch { }
                    }
                    q.Enqueue(child);
                }
            }
        }

        [Conditional("DEBUG")]
        private static void LogMemory(string tag)
        {
            try
            {
                var p = Process.GetCurrentProcess();
                Debug.WriteLine($"{tag}: Private={p.PrivateMemorySize64 / 1024 / 1024}MB, WS={p.WorkingSet64 / 1024 / 1024}MB");
            }
            catch { }
        }
    }
}
