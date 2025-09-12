using KIOSK.Services;
using KIOSK.ViewModels;
using Stateless;
using System.Diagnostics;
using System.Windows;
using System.Windows.Threading;

namespace KIOSK.FSM.MOCK
{
    public enum ExchangeState
    {
        Start,
        First,
        Second,
        Exit
    }

    // 단 4개의 트리거만 사용
    public enum ExchangeTrigger
    {
        Next,           // 다음 화면/단계로 진행
        Previous,       // 이전 화면으로 이동 ( History )
        Exit,           // 메인 화면(복귀)
        Error           // 에러 발생 -> 에러 화면
    }

    public partial class MockStateMachine
    {
        private readonly INavigationService _nav;
        private readonly IPopupService _popup;
        private readonly ILoggingService _logging;
        private readonly StateMachine<ExchangeState, ExchangeTrigger> _fsm;
        private readonly Stack<ExchangeState> _history = new();
        private readonly SemaphoreSlim _fireLock = new(1, 1);

        public MockStateMachine(INavigationService nav, IPopupService popup, ILoggingService logging)
        {
            _nav = nav;
            _popup = popup;
            _logging = logging;
            _fsm = new StateMachine<ExchangeState, ExchangeTrigger>(ExchangeState.Start);

            // 전이 로깅 및 후처리
            _fsm.OnTransitioned(async trigger =>
            {
                _logging.Info($"{trigger.Source} -> {trigger.Destination} via {trigger.Trigger}");

                // Previous로 전이 완료되면 스택에서 제거
                if (trigger.Trigger.Equals(ExchangeTrigger.Previous) && _history.Count > 0)
                {
                    _history.Pop();
                }

                // Exit로 전이되면 히스토리 초기화
                if (trigger.Destination == ExchangeState.Exit)
                {
                    _history.Clear();
                }

                await Task.CompletedTask;
            });

            ConfigureStates();
        }

        #region Fire wrappers (스레드 안전)
        private async Task FireAsyncSafe(ExchangeTrigger trigger)
        {
            await _fireLock.WaitAsync().ConfigureAwait(false);
            try
            {
                await _fsm.FireAsync(trigger).ConfigureAwait(false);    //ConfigureAwait, UI와 관련될 경우 사용 권장
            }
            catch (InvalidOperationException ex)
            {
                Debug.WriteLine($"[ExchangeSellStateMachine] invalid transition: {ex.Message}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ExchangeSellStateMachine] fire error: {ex}");
            }
            finally
            {
                _fireLock.Release();
            }
        }

        public async Task NextAsync()
        {
            // Start(초기 진입)에서 자동으로 Next를 호출할 때는 Start를 히스토리에 쌓지 않음.
            if (_fsm.State != ExchangeState.Start)
            {
                _history.Push(_fsm.State);
            }
            await FireAsyncSafe(ExchangeTrigger.Next);
        }

        public Task PreviousAsync() => FireAsyncSafe(ExchangeTrigger.Previous);

        public Task ExitAsync() => FireAsyncSafe(ExchangeTrigger.Exit);

        public Task ErrorAsync() => FireAsyncSafe(ExchangeTrigger.Error);
        #endregion

        private void ConfigureStates()
        {
            // Start -> Language (Next)
            _fsm.Configure(ExchangeState.Start)
                .OnEntryAsync(async () => await NextAsync())
                .Permit(ExchangeTrigger.Next, ExchangeState.First);

            // Language 화면
            _fsm.Configure(ExchangeState.First)
                .OnEntryAsync(async () =>
                {
                    await _nav.NavigateTo<ExchangeLanguageViewModel>(async vm =>
                    {
                        vm.OnStepMain = async () => await ExitAsync();
                        vm.OnStepPrevious = async () => await PreviousAsync();
                        vm.OnStepNext = async (bool? pass) => await NextAsync();
                        vm.OnStepError = async ex =>
                        {
                            _logging.Error(ex, ex.Message);
                            Debug.WriteLine(ex, "[ExchangeSellStateMachine] 언어 선택 중 오류 발생");
                            await ErrorAsync();
                        };

                        await Application.Current.Dispatcher.BeginInvoke(new Action(async () =>
                        {
                            await Task.Delay(10000); // 의도한 딜레이
                            await NextAsync();
                        }), DispatcherPriority.Background);
                    });
                })
                .Permit(ExchangeTrigger.Next, ExchangeState.Second)
                .Permit(ExchangeTrigger.Exit, ExchangeState.Exit)
                .PermitDynamic(ExchangeTrigger.Previous, () => _history.Count > 0 ? _history.Peek() : ExchangeState.Exit);

            // Currency 화면
            _fsm.Configure(ExchangeState.Second)
                .OnEntryAsync(async () =>
                {
                    await _nav.NavigateTo<ExchangeCurrencyViewModel>(async vm =>
                    {
                        vm.OnStepMain = async () => await ExitAsync();
                        vm.OnStepPrevious = async () => await PreviousAsync();
                        vm.OnStepNext = async (bool? res) => await NextAsync();
                        vm.OnStepError = async ex =>
                        {
                            _logging.Error(ex, ex.Message);
                            //Debug.WriteLine(ex, "[ExchangeSellStateMachine] 통화 선택 중 오류 발생");
                            await ErrorAsync();
                        };

                        await Application.Current.Dispatcher.BeginInvoke(new Action(async () =>
                        {
                            await Task.Delay(1000); // 의도한 딜레이
                            await NextAsync();
                        }), DispatcherPriority.Background);
                    });
                })
                .Permit(ExchangeTrigger.Next, ExchangeState.First)
                .Permit(ExchangeTrigger.Exit, ExchangeState.Exit)
                .PermitDynamic(ExchangeTrigger.Previous, () => _history.Count > 0 ? _history.Peek() : ExchangeState.Exit);

            // Exit (복귀 처리)
            _fsm.Configure(ExchangeState.Exit)
                .OnEntryAsync(async () =>
                {
                    _history.Clear();
                    await _nav.NavigateTo<ServiceViewModel>(vm => { /* 초기화 작업 필요 시 추가 */ });
                });
        }

        // 실제 API 호출 모의 (실제 구현으로 교체)
        private async Task<bool> CallExchangeApiAsync()
        {
            await Task.Delay(500); // simulate
            return true;
        }

        // 외부에서 호출 가능한 안전 래퍼들
        public Task StartAsync() => NextAsync(); // Start에서 Next로 이동
        public Task FireNextAsync() => NextAsync();
        public Task FirePreviousAsync() => PreviousAsync();
        public Task FireMainAsync() => ExitAsync();
        public Task FireErrorAsync() => ErrorAsync();

        // 테스트용
        public ExchangeState CurrentState => _fsm.State;
    }

}
