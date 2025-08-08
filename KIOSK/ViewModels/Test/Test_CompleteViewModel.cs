using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KIOSK.Managers;
using KIOSK.Services;
using Microsoft.Extensions.DependencyInjection;

namespace KIOSK.ViewModels;

public partial class Test_CompleteViewModel: ObservableObject
{
    private readonly Test_StateMachineService _stateMachine;
    private readonly IServiceProvider _provider;

    public Test_CompleteViewModel(Test_StateMachineService stateMachine, IServiceProvider provider)
    {
        _provider = provider;
        _stateMachine = stateMachine;

        var _manager = _provider.GetRequiredService<DeviceManager>();

        _manager.cmdPrint("");
    }
    
    [RelayCommand]
    private void Okay()
    {
        _stateMachine.GoToState(Test_StateMachineService.PageState.Home);
    }
    
    [RelayCommand]
    private void Cancel()
    {
        _stateMachine.GoToState(Test_StateMachineService.PageState.Scan);
    }
}