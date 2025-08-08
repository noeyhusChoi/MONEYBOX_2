using System.Diagnostics;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KIOSK.FSM;
using KIOSK.Models;
using KIOSK.Services;
using Microsoft.Extensions.DependencyInjection;

namespace KIOSK.ViewModels;

public partial class TestViewModel : ObservableObject
{
    private readonly Test_StateMachineService _stateMachine;
    private readonly TestStateMachine _st;
    private readonly IServiceProvider _provider;
    
    [ObservableProperty]
    private Uri source;

    public TestViewModel(Test_StateMachineService stateMachine, IServiceProvider provider, TestStateMachine st)
    {
        _st = st;
        _provider = provider;
        _stateMachine = stateMachine;
        Source = new Uri("Assets/Video/MoneyBoxVideo.mp4", UriKind.Relative); // 경로는 수정 가능
    }

    [RelayCommand]
    private void Go()
    {
        _st.Start();
        //_stateMachine.GoToState(Test_StateMachineService.PageState.Terms);
    }
    
    [RelayCommand]
    private async Task ApiTest()
    {
        var x = _provider.GetRequiredService<IApiService>();
        var result =  await x.SendCommandAsync("C011", null);

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        var model = _provider.GetRequiredService<ExchangeRateModel>();
        var response = JsonSerializer.Deserialize<ExchangeRateModel>(result, options);
        model.Result = response.Result;
        model.Data = response.Data;
        
        Console.WriteLine(result);
    }

    [RelayCommand]
    private async Task Loading()
    {
        var nav = _provider.GetRequiredService<INavigationService>();

        await nav.NavigateTo<LoadingViewModel>();
    }
}