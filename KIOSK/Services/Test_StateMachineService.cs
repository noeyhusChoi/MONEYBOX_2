using KIOSK.ViewModels;

namespace KIOSK.Services;

// TODO: 해당 클래스 제거 후 Stateless 패턴으로 대체
public class Test_StateMachineService
{
    public enum PageState
    {
        Home,
        ExchageRate,
        Terms,
        Scan,
        Complete
    }
    
    private readonly INavigationService _nav;

    // 필요한 경우 상태를 여기에 보관
    private PageState _state;

    public Test_StateMachineService(INavigationService nav)
    {
        _nav = nav;
        _state = PageState.Terms;
    }
    
    // 실제 페이지 전환 (private)
    public void GoToState(PageState state)
    {
        _state = state;
        switch (_state)
        {
            case PageState.Terms:
                _nav.NavigateTo<Test_TermsViewModel>();
                break;
            case PageState.ExchageRate:
                _nav.NavigateTo<ExchangeCurrencyViewModel>();
                break;
            case PageState.Scan:
                _nav.NavigateTo<Test_ScanViewModel>();
                break;
            case PageState.Complete:
                _nav.NavigateTo<Test_CompleteViewModel>();
                break;
            case PageState.Home:
                break;
        }
    }

    public void CW()
    {
        Console.WriteLine("TEST");
    }
}