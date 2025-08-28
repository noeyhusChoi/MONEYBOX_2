using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Localization;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;

namespace KIOSK.ViewModels;

public partial class Test_TermsViewModel : ObservableObject, IStepNext, IStepPrevious, IStepError
{
    private readonly ILocalizationService _localizationService;

    public Func<Task>? OnStepNext { get; set; }
    public Func<Task>? OnStepPrevious { get; set; }
    public Action<Exception>? OnStepError { get; set; }

    public Test_TermsViewModel(ILocalizationService localizationService)
    {
        _localizationService = localizationService;
    }

    [RelayCommand]
    private async Task Okay()
    {
        try
        {
            OnStepNext?.Invoke();
        }
        catch (Exception ex)
        {
            OnStepError?.Invoke(ex);
        }
    }

    [RelayCommand]
    private async Task Cancel()
    {
        try
        {
            OnStepPrevious?.Invoke();
        }
        catch(Exception ex)
        {
            OnStepError?.Invoke(ex);
        }
    }
}