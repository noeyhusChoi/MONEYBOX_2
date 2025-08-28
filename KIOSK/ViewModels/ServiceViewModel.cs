using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KIOSK.FSM;
using KIOSK.Models;
using KIOSK.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace KIOSK.ViewModels
{
    public partial class ServiceViewModel : ObservableObject
    {
        private readonly TestStateMachine _st;
        private readonly IServiceProvider _provider;

        [ObservableProperty]
        private Uri backgroundMp4Uri;

        public ServiceViewModel(IServiceProvider provider, TestStateMachine st)
        {
            _st = st;
            _provider = provider;
            BackgroundMp4Uri = new Uri("Assets/Video/MoneyBoxVideo.mp4", UriKind.Relative); // 경로는 수정 가능
        }

        [RelayCommand]
        private void Go()
        {
            _st.Start();
        }

        [RelayCommand]
        private async Task ApiTest()
        {
            var x = _provider.GetRequiredService<IApiService>();
            var result = await x.SendCommandAsync("C011", null);

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                NumberHandling = JsonNumberHandling.AllowReadingFromString
            };

            var model = _provider.GetRequiredService<ExchangeRateModel>();
            var response = JsonSerializer.Deserialize<ExchangeRateModel>(result, options);
            model.Result = response.Result;
            model.Data = response.Data;

            Debug.WriteLine(result);
        }

        [RelayCommand]
        private async Task Loading()
        {
            var nav = _provider.GetRequiredService<INavigationService>();
            await nav.NavigateTo<LoadingViewModel>();
        }

        [RelayCommand]
        private async Task Next(object? parameter)
        {
            if (parameter is string param)
            {
                switch (param)
                {
                    case "1":
                        var st = _provider.GetRequiredService<TestStateMachine>();
                        st.Start();
                        break;
                    case "2":
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
