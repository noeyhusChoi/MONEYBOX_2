using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KIOSK.Models;
using KIOSK.Services;
using Localization;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;

namespace KIOSK.ViewModels;

public partial class ExchangeCurrencyViewModel : ObservableObject, IStepMain, IStepNext, IStepPrevious, IStepError
{
    public Func<Task>? OnStepMain { get; set; }
    public Func<Task>? OnStepPrevious { get; set; }
    public Func<Task>? OnStepNext { get; set; }
    public Action<Exception>? OnStepError { get; set; }

    private readonly IServiceProvider _provider;
    
    [ObservableProperty] 
    private ObservableCollection<ExchangeRate> exchangeRates;

    [ObservableProperty]
    private ObservableCollection<Uri> flagUri;

    [ObservableProperty]
    private int _rows = 3;

    public ExchangeCurrencyViewModel(IServiceProvider provider)
    {
        _provider = provider;

        var exchangeRateModel = _provider.GetRequiredService<ExchangeRateModel>();
        var excludeExchangeRateList = new[] { "RUB" };      // ������ ��ȭ ��� (��ҹ��� ���� ����)   

        exchangeRates = new ObservableCollection<ExchangeRate>(
            exchangeRateModel.Data.Where(er => !excludeExchangeRateList.Contains(er.Currency, StringComparer.OrdinalIgnoreCase))
        );
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
        if (parameter is string param)
        {
            Debug.WriteLine($"Selected Currency: {param}");
            try
            {
                OnStepNext?.Invoke();
            }
            catch (Exception ex)
            {
                OnStepError?.Invoke(ex);
            }
        }
    }
}