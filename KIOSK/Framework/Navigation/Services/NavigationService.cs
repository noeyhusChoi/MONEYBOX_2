using KIOSK.Utils;
using KIOSK.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace KIOSK.Services;

public interface INavigationService
{
    T GetViewModel<T>() where T : class;

    Task NavigateTo<T>() where T : class;
    Task NavigateTo<T>(Action<T> initializer) where T : class;
    Task NavigateTo<T>(Action<T>? initializer, object? parameter) where T : class;

    Task NavigateWithLoadingAsync<TLoading, TTarget>(
        Action<TTarget>? initializer = null,
        object? parameter = null)
        where TLoading : class
        where TTarget : class;
}

public sealed class NavigationService : INavigationService
{
    private readonly ILoggingService _logging;
    private readonly IServiceProvider _provider; // 루트 Provider (싱글톤)

    // 네비게이션별 취소 토큰 (빠른 재전환 시 이전 로딩 취소용)
    private CancellationTokenSource? _navCts;

    // 현재 화면에 대한 DI Scope
    private IServiceScope? _currentScope;

    public NavigationService(IServiceProvider provider, ILoggingService logging)
    {
        _provider = provider;
        _logging = logging;
    }

    // 주의: 이건 "현재 화면용 Scope"가 아닌, 그냥 루트 Provider에서 꺼내는 헬퍼.
    // 네비게이션과 무관하게 쓸 때만 사용.
    public T GetViewModel<T>() where T : class
        => _provider.GetRequiredService<T>();

    // 기본 Navigate
    public Task NavigateTo<T>() where T : class
        => NavigateCoreAsync<T>(initializer: null, parameter: null);

    public Task NavigateTo<T>(Action<T> initializer) where T : class
        => NavigateCoreAsync<T>(initializer, parameter: null);

    public Task NavigateTo<T>(Action<T>? initializer, object? parameter) where T : class
        => NavigateCoreAsync<T>(initializer, parameter);

    // 로딩 화면 → 실제 화면 전환 패턴
    public async Task NavigateWithLoadingAsync<TLoading, TTarget>(
        Action<TTarget>? initializer = null,
        object? parameter = null)
        where TLoading : class
        where TTarget : class
    {
        var mainVm = _provider.GetRequiredService<MainShellViewModel>();

        // 1) 이전 화면 정리 (OnUnload + Scope Dispose + Cancel)
        await CleanupPreviousAsync(mainVm);

        // 2) 새 Scope 생성 (로딩/타겟 둘 다 이 Scope에서 생성)
        _currentScope = _provider.CreateScope();
        var scopeProvider = _currentScope.ServiceProvider;

        // 3) 로딩 화면 VM 생성 및 표시
        var loadingVm = scopeProvider.GetRequiredService<TLoading>();
        mainVm.NavigateAction?.Invoke(loadingVm);

        // 4) 실제 대상 VM 준비
        var vm = scopeProvider.GetRequiredService<TTarget>();
        initializer?.Invoke(vm);

        _navCts = new CancellationTokenSource();
        var ct = _navCts.Token;

        try
        {
            // 진입 훅 (타겟 VM의 OnLoad)
            if (vm is INavigable nav)
                await nav.OnLoadAsync(parameter, ct);

            // 5) 초기화 끝나면 실제 화면으로 전환
            mainVm.NavigateAction?.Invoke(vm);

            LogNavigated(null, typeof(TTarget).Name);
        }
        catch (OperationCanceledException)
        {
            _logging.Warn("Navigation canceled.");
        }
        catch (Exception ex)
        {
            _logging.Error(ex, "NavigateWithLoadingAsync failed");
            throw;
        }
    }

    // === 내부 구현 ===

    private async Task NavigateCoreAsync<T>(Action<T>? initializer, object? parameter) where T : class
    {
        var mainVm = _provider.GetRequiredService<MainShellViewModel>();
        var prev = mainVm.CurrentViewModel;
        var prevName = prev?.GetType().Name;

        try
        {
            // 1) 이전 화면 정리 (OnUnload + Scope Dispose + Cancel)
            await CleanupPreviousAsync(mainVm);

            // 2) 새 Scope 생성
            _currentScope = _provider.CreateScope();
            var scopeProvider = _currentScope.ServiceProvider;

            // 3) 새 VM 생성 + 초기화 주입
            var vm = scopeProvider.GetRequiredService<T>();
            initializer?.Invoke(vm);

            // 4) 화면 교체
            mainVm.NavigateAction?.Invoke(vm);

            // 5) INavigable이면 진입 훅 호출
            _navCts = new CancellationTokenSource();
            var ct = _navCts.Token;

            if (vm is INavigable nav)
            {
                try
                {
                    await nav.OnLoadAsync(parameter, ct);
                }
                catch (OperationCanceledException)
                {
                    _logging.Warn("Navigation canceled.");
                }
            }

            LogNavigated(prevName, typeof(T).Name);
        }
        catch (Exception ex)
        {
            _logging.Error(ex, "Navigation failed");
            throw;
        }
    }

    private async Task CleanupPreviousAsync(MainShellViewModel mainVm)
    {
        // 1) 이전 네비게이션 작업 취소
        _navCts?.Cancel();
        _navCts?.Dispose();
        _navCts = null;

        // 2) 이전 VM 정리 (OnUnloadAsync 호출)
        if (mainVm.CurrentViewModel is INavigable oldNav)
        {
            try
            {
                await oldNav.OnUnloadAsync();
            }
            catch (Exception e)
            {
                _logging.Warn("OnUnloadAsync failed: " + e.Message);
            }
        }

        // 3) 이전 Scope 정리 (여기서 기존 ViewModel/서비스 Dispose)
        if (_currentScope is not null)
        {
            _currentScope.Dispose();
            _currentScope = null;
        }
    }

    private void LogNavigated(string? prevName, string nextName)
    {
        if (!string.IsNullOrEmpty(prevName))
            _logging.Info($"Navigated ({prevName} >> {nextName})");
        else
            _logging.Info($"Navigated to [{nextName}] without previous ViewModel");
    }
}
