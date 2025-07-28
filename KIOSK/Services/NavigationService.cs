using System.Windows.Controls;
using KIOSK.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace KIOSK.Services;

public interface INavigationService
{
    T GetViewModel<T>() where T : class;
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
}

