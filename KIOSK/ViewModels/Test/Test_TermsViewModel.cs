using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace KIOSK.ViewModels;

public partial class Test_TermsViewModel : ObservableObject, IStepNext, IStepPrevious, IStepError
{

    public Func<Task>? OnStepNext { get; set; }
    public Func<Task>? OnStepPrevious { get; set; }
    public Action<Exception>? OnStepError { get; set; }


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