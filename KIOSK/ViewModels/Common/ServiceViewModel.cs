using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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
            var db = _provider.GetRequiredService<IDataBaseService>();

            try
            {

                var dt1 = await db.QueryAsync<DataTable>("SELECT * FROM KIOSK", type: CommandType.Text);
                var dt2 = await db.QueryAsync<DataSet>("sp_get_all_kiosks", type: CommandType.StoredProcedure);

                foreach (DataTable x in dt2.Tables)
                {
                    Debug.WriteLine(x.Columns.Count);
                }
                //dt에서 값 가져오기
                if (dt1 != null)
                    _logging.Info($"GET DB DATA {dt1.Rows[0]["KIOSK_ID"]}");

                if (dt2 != null)
                    _logging.Info($"GET DB DATA {dt1.Rows[0]["KIOSK_ID"]}");
            }
            catch
            {
                _logging.Info($"GET DB DATA FAILED");
            }

            try
            {
                var billPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Sound", "Click.wav");
                var audio = _provider.GetRequiredService<IAudioService>();
                audio.Play(billPath);
            }
            catch
            {
                _logging.Info($"PLAY AUDIO FAILED");
            }

            //var nav = _provider.GetRequiredService<INavigationService>();
            //await nav.NavigateTo<ExchangeResultViewModel>();
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
