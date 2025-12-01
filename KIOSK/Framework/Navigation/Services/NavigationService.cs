using KIOSK.Modules.Shells.Interface;
using KIOSK.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace KIOSK.Services;

// ==========================================
// Public API
// ==========================================
public interface INavigationService
{
    void AttachTopShell(ITopShellHost shell);

    // TopShell 전환 (RootShell <-> EnvironmentShell)
    Task SwitchTopShell<TTopShell>()
        where TTopShell : class, ITopShellHost;

    // SubShell 전환 (ServiceShell, ExchangeShell, GtfShell)
    Task SwitchSubShell<TSubShell>()
        where TSubShell : class, ISubShellHost;

    // Flow 전환
    Task NavigateTo<TView>(Action<TView>? init = null, object? parameter = null)
        where TView : class;

    // 기존과 동일한 기본 기능
    T GetViewModel<T>() where T : class;

    ITopShellHost? ActiveTopShell { get; }
    ISubShellHost? ActiveSubShell { get; }
    object? ActiveFlowView { get; }
}

public sealed class NavigationService : INavigationService
{
    private readonly IServiceProvider _provider;
    private readonly ILoggingService _logging;

    public NavigationService(IServiceProvider provider, ILoggingService logging)
    {
        _provider = provider;
        _logging = logging;
    }

    // Shell Layer State
    public ITopShellHost? ActiveTopShell { get; private set; }
    public ISubShellHost? ActiveSubShell { get; private set; }
    public object? ActiveFlowView { get; private set; }

    // Flow Scope
    private IServiceScope? _flowScope;
    private CancellationTokenSource? _cts;

    // 0. 최초 RootShell 등록
    public void AttachTopShell(ITopShellHost shell)
    {
        // RootShellViewModel에서 호출됨
        ActiveTopShell = shell;
    }

    // 1. TopShell 전환
    public async Task SwitchTopShell<TTopShell>()
        where TTopShell : class, ITopShellHost
    {
        CleanupFlowScope();
        CleanupSubShell();
        CleanupTopShell();

        // MainShell / Admin
        ActiveTopShell = _provider.GetRequiredService<TTopShell>();

        if (ActiveTopShell is INavigable nav)
            await nav.OnLoadAsync(null, CancellationToken.None);
    }

    // 2. SubShell 전환
    private IServiceScope? _subShellScope;

    public async Task SwitchSubShell<TSubShell>()
        where TSubShell : class, ISubShellHost
    {
        if (ActiveTopShell == null)
            throw new InvalidOperationException("TopShell이 설정되지 않았습니다.");

        CleanupFlowScope();
        CleanupSubShell();

        _subShellScope?.Dispose();
        _subShellScope = _provider.CreateScope();

        var subShell = _subShellScope.ServiceProvider.GetRequiredService<TSubShell>();

        ActiveSubShell = subShell;
        ActiveTopShell.SetSubShell(subShell);

        if (subShell is INavigable nav)
            await nav.OnLoadAsync(null, CancellationToken.None);
    }

    // 3. FlowView 전환 (SubShell 내부 화면)
    public async Task NavigateTo<TView>(
        Action<TView>? init = null,
        object? parameter = null)
        where TView : class
    {
        if (ActiveSubShell == null)
            throw new InvalidOperationException("SubShell이 설정되지 않았습니다.");

        CleanupFlowScope();

        _flowScope = _provider.CreateScope();
        var vm = _flowScope.ServiceProvider.GetRequiredService<TView>();

        init?.Invoke(vm);

        ActiveSubShell.SetInnerView(vm);
        ActiveFlowView = vm;

        _cts = new CancellationTokenSource();

        if (vm is INavigable nav)
            await nav.OnLoadAsync(parameter, _cts.Token);
    }

    // 4. Cleanup Helpers
    private void CleanupTopShell()
    {
        if (ActiveTopShell is INavigable nav)
            nav.OnUnloadAsync().Wait();

        ActiveTopShell = null;
    }


    private void CleanupSubShell()
    {
        if (ActiveSubShell is INavigable nav)
            nav.OnUnloadAsync().Wait();

        ActiveTopShell?.SetSubShell(null);
        ActiveSubShell = null;
    }


    private void CleanupFlowScope()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;

        if (ActiveFlowView is INavigable nav)
            nav.OnUnloadAsync().Wait();

        _flowScope?.Dispose();
        _flowScope = null;

        ActiveFlowView = null;
    }

    // 5. 기본 팩토리 기능
    public T GetViewModel<T>() where T : class =>
        _provider.GetRequiredService<T>();
}

