using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KIOSK.FSM;
using KIOSK.Models;
using KIOSK.Services;
using Localization;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace KIOSK.ViewModels;

public partial class ExchangeLanguageViewModel : ObservableObject, IStepMain, IStepNext, IStepPrevious, IStepError
{
    public Func<Task>? OnStepMain { get; set; }
    public Func<Task>? OnStepPrevious { get; set; }
    public Func<bool?, Task>? OnStepNext { get; set; }
    public Action<Exception>? OnStepError { get; set; }

    private readonly IServiceProvider _provider;

    [ObservableProperty]
    private Uri source;

    public ExchangeLanguageViewModel(IServiceProvider provider)
    {
        _provider = provider;

        var billPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Sound", "Click.wav");
        var audio = _provider.GetRequiredService<IAudioService>();
        audio.Play(billPath);
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
    private async Task Main()
    {
        try
        {
            OnStepMain?.Invoke();
        }
        catch (Exception ex)
        {
            OnStepError?.Invoke(ex);
        }
    }

    [RelayCommand]
    private async Task Previous()
    {
        try
        {
            OnStepPrevious?.Invoke();
        }
        catch (Exception ex)
        {
            OnStepError?.Invoke(ex);
        }
    }

    [RelayCommand]
    private async Task Next(object? parameter)
    {
        var lang = _provider.GetRequiredService<ILocalizationService>();
        if (parameter is string param)
        {
            try
            {
                CultureInfo culture = new CultureInfo("ko-KR");
                switch (param)
                {
                    case "1":
                        culture = new CultureInfo("en-US");
                        break;
                    case "2":
                        culture = new CultureInfo("zh-CN");
                        break;
                    case "3":
                        culture = new CultureInfo("zh-TW");
                        break;
                    case "4":
                        culture = new CultureInfo("ja-JP");
                        break;
                    case "5":
                        culture = new CultureInfo("ko-KR");
                        break;
                }
                lang.SetCulture(culture);

                OnStepNext?.Invoke(true);
            }
            catch (Exception ex)
            {
                OnStepError?.Invoke(ex);
            }
        }
    }
}