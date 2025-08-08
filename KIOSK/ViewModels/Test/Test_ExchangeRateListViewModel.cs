using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KIOSK.Models;
using KIOSK.Services;
using Microsoft.Extensions.DependencyInjection;

namespace KIOSK.ViewModels;

public partial class Test_ExchangeRateListViewModel : ObservableObject
{
    private readonly Test_StateMachineService _stateMachine;
    private readonly IServiceProvider _provider;
    
    [ObservableProperty] 
    private ObservableCollection<ExchangeRate> exchangeRates;

    [ObservableProperty]
    private int _rows = 3;

    public Test_ExchangeRateListViewModel(Test_StateMachineService stateMachine, IServiceProvider provider)
    {
        _provider = provider;
        _stateMachine = stateMachine;

        var x = _provider.GetRequiredService<ExchangeRateModel>();
        exchangeRates = x.Data;
    }

    [RelayCommand]
    private void Okay()
    {
        _stateMachine.GoToState(Test_StateMachineService.PageState.Scan);
    }

    [RelayCommand]
    private void Cancel()
    {
        _stateMachine.GoToState(Test_StateMachineService.PageState.Terms);
    }
}