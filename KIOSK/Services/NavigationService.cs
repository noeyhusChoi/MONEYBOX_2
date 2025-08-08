using KIOSK.Utils;
using KIOSK.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace KIOSK.Services;

public interface INavigationService
{
    T GetViewModel<T>() where T : class;
    public Task NavigateTo<T>() where T : class;

    public Task NavigateTo<T>(Action<T> initializer) where T : class;
}


public class NavigationService : INavigationService
{
    private readonly IServiceProvider _provider;

    public NavigationService(IServiceProvider provider)
    {
        _provider = provider;
    }

    public T GetViewModel<T>() where T : class
        => _provider.GetRequiredService<T>();

    public async Task NavigateTo<T>() where T : class
    {
        try
        {
            var viewModel = _provider.GetRequiredService<T>();
            var mainVm = _provider.GetRequiredService<MainViewModel>();
            var preViewModel = mainVm.CurrentViewModel.GetType().Name;

            // 1. 로딩 화면 먼저 표시
            var vm = _provider.GetRequiredService<LoadingViewModel>();
            mainVm.NavigateAction?.Invoke(vm);

            // 2. 짧은 지연 (혹은 실제 데이터 로딩)
            await Task.Delay(1500); // 또는 await LoadAsync();

            // 3. 실제 뷰모델로 전환
            mainVm.NavigateAction?.Invoke(viewModel);

            //preViewModel 타입 출력
            if (preViewModel != null)
            {
                CustomLog.WriteLine($"Navigated from [{preViewModel}] to [{typeof(T).Name}]");
            }
            else
            {
                CustomLog.WriteLine($"Navigated to [{typeof(T).Name}] without previous ViewModel");
            }
        }
        catch(Exception ex)
        {
            CustomLog.WriteLine(ex.Message);
        }
    }

    public async Task NavigateTo<T>(Action<T> initializer) where T : class
    {
        var viewModel = _provider.GetRequiredService<T>();
        initializer?.Invoke(viewModel);
        var mainVm = _provider.GetRequiredService<MainViewModel>();
        var preViewModel = mainVm.CurrentViewModel.GetType().Name;

        // 1. 로딩 화면 먼저 표시
        var vm = _provider.GetRequiredService<LoadingViewModel>();
        mainVm.NavigateAction?.Invoke(vm);

        // 2. 짧은 지연 (혹은 실제 데이터 로딩)
        await Task.Delay(1500); // 또는 await LoadAsync();

        // 3. 실제 뷰모델로 전환
        mainVm.NavigateAction?.Invoke(viewModel);

        //preViewModel 타입 출력
        if (preViewModel != null)
        {
            CustomLog.WriteLine($"Navigated from [{preViewModel}] to [{typeof(T).Name}]");
        }
        else
        {
            CustomLog.WriteLine($"Navigated to [{typeof(T).Name}] without previous ViewModel");
        }
    }

}

