using KIOSK.Managers;
using KIOSK.Models;
using KIOSK.Services;
using KIOSK.ViewModels;
using Microsoft.Extensions.Logging;
using Stateless;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KIOSK.FSM
{
    public enum ExchangeState
    {
        Start,
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
        TermsAccepted,
        DepositDone,
        ApiSuccess,
        ErrorOccurred,
        Cancel
    }

    public partial class TestStateMachine
    {
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
            _fsm.Configure(ExchangeState.Start)                         // 작업 조건 : ExchangeState.Start일 때,
                .Permit(ExchangeTrigger.Start, ExchangeState.Terms);    // 트리거 작업 등록 : ExchangeTrigger.Start이면 ExchangeState.Terms 변경 

            // Terms
            _fsm.Configure(ExchangeState.Terms)                         // ExchangeState.Start일 때,
            .OnEntryAsync(async () =>                                   // 실행
             {
                 Debug.WriteLine("Terms");
                 await _nav.NavigateTo<Test_TermsViewModel>(vm =>
                 {
                     vm.OnStepNext = async () => await FireAsync(ExchangeTrigger.TermsAccepted);
                     vm.OnStepPrevious = async () => await FireAsync(ExchangeTrigger.Cancel);
                     vm.OnStepError = async ex =>
                     {
                         Debug.WriteLine(ex, "약관 동의 중 오류 발생");
                         await FireAsync(ExchangeTrigger.ErrorOccurred);
                     };
                 });
             })
            .Permit(ExchangeTrigger.TermsAccepted, ExchangeState.Deposit)
            .Permit(ExchangeTrigger.ErrorOccurred, ExchangeState.Error)
            .Permit(ExchangeTrigger.Cancel, ExchangeState.Cancelled);

            _fsm.Configure(ExchangeState.Deposit)
                .OnEntryAsync(async () =>
                {

                    Debug.WriteLine("Deposit");
                    // 여기에 입금 관련 로직을 추가합니다.
                    // 예: await _device.OpenDepositAsync();
                    FireAsync(ExchangeTrigger.DepositDone);
                })
                .Permit(ExchangeTrigger.DepositDone, ExchangeState.Terms);

            _fsm.Configure(ExchangeState.Cancelled)
                .OnEntryAsync(async () =>
                {
                    Debug.WriteLine("Cancelled");
                    await _nav.NavigateTo<TestViewModel>();
                })
                .Permit(ExchangeTrigger.Start, ExchangeState.Start);

            //_fsm.Configure(ExchangeState.Deposit)
            //    .OnEntryAsync(async () =>
            //    {
            //        //await _device.OpenDepositAsync();
            //        Fire(ExchangeTrigger.DepositDone);
            //    })
            //    .Permit(ExchangeTrigger.DepositDone, ExchangeState.ApiRequest);
        }

        public void Fire(ExchangeTrigger trigger) => _fsm.Fire(trigger);
        public async Task FireAsync(ExchangeTrigger trigger) => await _fsm.FireAsync(trigger);
        public void Start() => _fsm.FireAsync(ExchangeTrigger.Start);
    }
}
