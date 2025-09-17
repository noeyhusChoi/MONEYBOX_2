using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Devices.Abstractions;
using Devices.Core;
using KIOSK.FSM;
using KIOSK.Models;
using KIOSK.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace KIOSK.ViewModels
{
    public partial class ServiceViewModel : ObservableObject
    {
        private readonly IServiceProvider _provider;
        private readonly ExchangeSellStateMachine _st;
        private readonly ILoggingService _logging;

        [ObservableProperty]
        private Uri backgroundMp4Uri;

        public ServiceViewModel(IServiceProvider provider, ExchangeSellStateMachine st, ILoggingService logging)
        {
            _provider = provider;
            _st = st;
            _logging = logging;
            BackgroundMp4Uri = new Uri("Assets/Video/MoneyBoxVideo.mp4", UriKind.Relative); // 경로는 수정 가능
        }

        [RelayCommand]
        private async Task Go()
        {
            // TEST CODE

            //var db = _provider.GetRequiredService<IDataBaseService>();

            var deviceManager = _provider.GetRequiredService<DeviceManager>();


            var res = await deviceManager.SendAsync("PRINTER1", new DeviceCommand("Cut", new byte[] { 0x1B, 0x69 }));
            Debug.WriteLine($"{res.Success} {res.Message}");
        }

        [RelayCommand]
        private async Task ApiTest()
        {
            try
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

                _logging.Info("GET API DATA EXCHANGERATE");
            }
            catch (Exception ex)
            {
                _logging.Info("GET API DATA FAILED");
            }

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
            var billPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Sound", "Click.wav");

            var audio = _provider.GetRequiredService<IAudioService>();
            audio.Play(billPath);

            if (parameter is string param)
            {
                switch (param)
                {
                    case "1":
                        var st = _provider.GetRequiredService<ExchangeSellStateMachine>();
                        await st.StartAsync();

                        //var mst = _provider.GetRequiredService<FSM.MOCK.MockStateMachine>();
                        //await mst.StartAsync();
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
