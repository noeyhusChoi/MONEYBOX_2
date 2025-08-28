using KIOSK.Services;
using KIOSK.ViewModels;
using Stateless;
using System.Diagnostics;

namespace KIOSK.FSM
{
    public enum ExchangeState
    {
        // 시작 -> 언어선택 -> 약관 동의 -> 입금 -> API 요청 -> 완료
        Start,
        Language,
        Currency,
        Terms,
        Deposit,
        ApiRequest,
        Complete,
        Error,
        Cancelled
    }

    public enum ExchangeTrigger
    {
        Start,
        CurrencySelected,
        LanguageSelected,
        TermsAccepted,
        DepositDone,
        ApiSuccess,
        ErrorOccurred,
        Cancel
    }

    public partial class TestStateMachine
    {
        // TODO: 이전 상태 저장(뒤로 가기용)
        private readonly StateMachine<ExchangeState, ExchangeTrigger> _fsm;
        private readonly INavigationService _nav;

        public TestStateMachine(INavigationService nav)
        {
            _nav = nav;
            _fsm = new StateMachine<ExchangeState, ExchangeTrigger>(ExchangeState.Start);

            ConfigureStates();
        }

        private void ConfigureStates()
        {
            // Start
            _fsm.Configure(ExchangeState.Start) // 작업 조건 : ExchangeState.Start일 때,
                .Permit(ExchangeTrigger.Start, ExchangeState.Language); // 트리거 작업 등록 : ExchangeTrigger.Start이면 ExchangeState.Terms 변경 

            // 언어 선택 화면
            _fsm.Configure(ExchangeState.Language)
                .OnEntryAsync(async () =>   // 실행
                {
                    await _nav.NavigateTo<ExchangeLanguageViewModel>(vm =>
                    {
                        vm.OnStepMain = async () => await FireAsync(ExchangeTrigger.Cancel);   // 메인 화면으로 돌아가기
                        vm.OnStepPrevious = async () => await FireAsync(ExchangeTrigger.Cancel); // 이전 화면으로 돌아가기 (취소)
                        vm.OnStepNext = async () => await FireAsync(ExchangeTrigger.LanguageSelected);
                        vm.OnStepError = async ex =>
                        {
                            Debug.WriteLine(ex, "언어 선택 중 오류 발생");
                            await FireAsync(ExchangeTrigger.ErrorOccurred);
                        };
                    });
                })
                .Permit(ExchangeTrigger.LanguageSelected, ExchangeState.Currency)
                .Permit(ExchangeTrigger.Cancel, ExchangeState.Cancelled)
                .Permit(ExchangeTrigger.ErrorOccurred, ExchangeState.Error);

            // 통화 선택 화면
            _fsm.Configure(ExchangeState.Currency)
                .OnEntryAsync(async () =>   // 실행
                {
                    // 환전 거래 선택 후 시도 하는 방향, 현재는 ExchangeCurrencyViewModel 진행
                    // 환율 조회 API 호출
                    // 성공 시 다음 화면
                    // 실패 시 마지막 환율 정보 갱신 시간 비교 (예, 5분 이내면 계속 진행, 아니면 에러 화면)

                    await _nav.NavigateTo<ExchangeCurrencyViewModel>(vm =>
                    {
                        vm.OnStepMain = async () => await FireAsync(ExchangeTrigger.Cancel);   // 메인 화면으로 돌아가기
                        vm.OnStepPrevious = async () => await FireAsync(ExchangeTrigger.Start); // 이전 화면으로 돌아가기 (언어 선택 화면)
                        vm.OnStepNext = async () => await FireAsync(ExchangeTrigger.CurrencySelected);
                        vm.OnStepError = async ex =>
                        {
                            Debug.WriteLine(ex, "통화 선택 중 오류 발생");
                            await FireAsync(ExchangeTrigger.ErrorOccurred);
                        };
                    });
                })
                .Permit(ExchangeTrigger.CurrencySelected, ExchangeState.Terms)
                .Permit(ExchangeTrigger.Cancel, ExchangeState.Cancelled)
                .Permit(ExchangeTrigger.Start, ExchangeState.Language)
                .Permit(ExchangeTrigger.ErrorOccurred, ExchangeState.Error);

            // 약관 동의
            _fsm.Configure(ExchangeState.Terms)                         
            .OnEntryAsync(async () =>                                   // 실행
             {
                 await _nav.NavigateTo<ExchangeTermsViewModel>(vm =>
                 {
                     vm.OnStepMain = async () => await FireAsync(ExchangeTrigger.Cancel);   // 메인 화면으로 돌아가기
                     vm.OnStepPrevious = async () => await FireAsync(ExchangeTrigger.LanguageSelected); // 이전 화면으로 돌아가기 (통화 선택 화면)
                     vm.OnStepNext = async () => await FireAsync(ExchangeTrigger.TermsAccepted);
                     vm.OnStepError = async ex =>
                     {
                         Debug.WriteLine(ex, "약관 동의 중 오류 발생");
                         await FireAsync(ExchangeTrigger.ErrorOccurred);
                     };
                 });
             })
            .Permit(ExchangeTrigger.TermsAccepted, ExchangeState.Deposit)
            .Permit(ExchangeTrigger.Cancel, ExchangeState.Cancelled)
            .Permit(ExchangeTrigger.LanguageSelected, ExchangeState.Currency)
            .Permit(ExchangeTrigger.ErrorOccurred, ExchangeState.Error);

            _fsm.Configure(ExchangeState.Deposit)
            .OnEntryAsync(async () =>
            {
                Debug.WriteLine("Next");
                await FireAsync(ExchangeTrigger.CurrencySelected);
            })                       // 실행
            .Permit(ExchangeTrigger.CurrencySelected, ExchangeState.Terms);

            // 취소
            _fsm.Configure(ExchangeState.Cancelled)
            .OnEntryAsync(async () =>
            {
                Debug.WriteLine("Cancelled");
                await _nav.NavigateTo<ServiceViewModel>(vm =>
                {
                    // 초기화 작업이 필요하면 여기에 추가
                });
            })
            .Permit(ExchangeTrigger.Start, ExchangeState.Start);
        }

        public void Fire(ExchangeTrigger trigger) => _fsm.Fire(trigger);
        public async Task FireAsync(ExchangeTrigger trigger) => await _fsm.FireAsync(trigger);
        public void Start() => _fsm.FireAsync(ExchangeTrigger.Start);
    }
}
