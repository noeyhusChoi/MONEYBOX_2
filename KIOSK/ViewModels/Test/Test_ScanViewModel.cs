using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KIOSK.Services;

namespace KIOSK.ViewModels;

public partial class Test_ScanViewModel: ObservableObject
{
    private readonly Test_StateMachineService _stateMachine;
    
    public Test_ScanViewModel(Test_StateMachineService stateMachine)
    {
        _stateMachine = stateMachine;
    }
    
    [RelayCommand]
    private void Okay()
    {
        _stateMachine.GoToState(Test_StateMachineService.PageState.Complete);
    }
    
    [RelayCommand]
    private void Cancel()
    {
        _stateMachine.GoToState(Test_StateMachineService.PageState.Terms);
    }
}