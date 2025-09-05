using KIOSK.Services;
using KIOSK.ViewModels;
using KIOSK.ViewModels.Exchange;
using KIOSK.ViewModels.Exchange.Popup;
using Stateless;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.DirectoryServices.ActiveDirectory;
using System.Threading;
using System.Threading.Tasks;

namespace KIOSK.FSM
{
    public enum ExchangeState
    {
        Start,
        Language,
        Currency,
        Terms,
        IDScan,
        IDScanning,
        IDScanningComplete,
        Deposit,
        ApiRequest,
        Complete,
        Error,
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

    public partial class ExchangeSellStateMachine
    {
        private readonly StateMachine<ExchangeState, ExchangeTrigger> _fsm;
        private readonly INavigationService _nav;
        private readonly IPopupService _popup;
        private readonly Stack<ExchangeState> _history = new();
        private readonly SemaphoreSlim _fireLock = new(1, 1);

        public ExchangeSellStateMachine(INavigationService nav, IPopupService popup)
        {
            _nav = nav;
            _popup = popup;
            _fsm = new StateMachine<ExchangeState, ExchangeTrigger>(ExchangeState.Start);

            // 전이 로깅 및 후처리
            _fsm.OnTransitioned(async trigger =>
            {
                Debug.WriteLine($"[ExchangeSellStateMachine] {trigger.Source} -> {trigger.Destination} via {trigger.Trigger}");
            
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
                .Permit(ExchangeTrigger.Next, ExchangeState.Language);

            // Language 화면
            _fsm.Configure(ExchangeState.Language)
                .OnEntryAsync(async () =>
                {
                    await _nav.NavigateTo<ExchangeLanguageViewModel>(vm =>
                    {
                        vm.OnStepMain = async () => await ExitAsync();
                        vm.OnStepPrevious = async () => await PreviousAsync();
                        vm.OnStepNext = async (bool? pass) => await NextAsync();
                        vm.OnStepError = async ex =>
                        {
                            Debug.WriteLine(ex, "[ExchangeSellStateMachine] 언어 선택 중 오류 발생");
                            await ErrorAsync();
                        };
                    });
                })
                .Permit(ExchangeTrigger.Next, ExchangeState.Currency)
                .Permit(ExchangeTrigger.Exit, ExchangeState.Exit)
                .Permit(ExchangeTrigger.Error, ExchangeState.Error)
                // Previous 는 히스토리 기반으로 동작: PermitDynamic으로 모든 State에서 처리
                .PermitDynamic(ExchangeTrigger.Previous, () => _history.Count > 0 ? _history.Peek() : ExchangeState.Exit);

            // Currency 화면
            _fsm.Configure(ExchangeState.Currency)
                .OnEntryAsync(async () =>
                {
                    await _nav.NavigateTo<ExchangeCurrencyViewModel>(vm =>
                    {
                        vm.OnStepMain = async () => await ExitAsync();
                        vm.OnStepPrevious = async () => await PreviousAsync();
                        vm.OnStepNext = async (bool? res) => await NextAsync();
                        vm.OnStepError = async ex =>
                        {
                            Debug.WriteLine(ex, "[ExchangeSellStateMachine] 통화 선택 중 오류 발생");
                            await ErrorAsync();
                        };
                    });
                })
                .Permit(ExchangeTrigger.Next, ExchangeState.Terms)
                .Permit(ExchangeTrigger.Exit, ExchangeState.Exit)
                .Permit(ExchangeTrigger.Error, ExchangeState.Error)
                .PermitDynamic(ExchangeTrigger.Previous, () => _history.Count > 0 ? _history.Peek() : ExchangeState.Exit);

            // Terms 화면
            _fsm.Configure(ExchangeState.Terms)
                .OnEntryAsync(async () =>
                {
                    await _nav.NavigateTo<ExchangeTermsViewModel>(vm =>
                    {
                        vm.OnStepMain = async () => await ExitAsync();
                        vm.OnStepPrevious = async () => await PreviousAsync();
                        vm.OnStepNext = async (bool? res) => await NextAsync();
                        vm.OnStepError = async ex =>
                        {
                            Debug.WriteLine(ex, "[ExchangeSellStateMachine] 약관 동의 중 오류 발생");
                            await ErrorAsync();
                        };
                    });
                })
                .Permit(ExchangeTrigger.Next, ExchangeState.IDScan)
                .Permit(ExchangeTrigger.Exit, ExchangeState.Exit)
                .Permit(ExchangeTrigger.Error, ExchangeState.Error)
                .PermitDynamic(ExchangeTrigger.Previous, () => _history.Count > 0 ? _history.Peek() : ExchangeState.Exit);

            // IDScan (준비 화면)
            _fsm.Configure(ExchangeState.IDScan)
                .OnEntryAsync(async () =>
                {
                    await _nav.NavigateTo<ExchangeIDScanViewModel>(vm =>
                    {
                        vm.OnStepMain = async () => await ExitAsync();
                        vm.OnStepPrevious = async () => await PreviousAsync();
                        vm.OnStepNext = async (bool? res) => await NextAsync();
                        vm.OnStepError = async ex =>
                        {
                            Debug.WriteLine(ex, "[ExchangeSellStateMachine] 신분증 스캔 준비 오류 발생");
                            await ErrorAsync();
                        };
                    });
                })
                .Permit(ExchangeTrigger.Next, ExchangeState.IDScanning)
                .Permit(ExchangeTrigger.Exit, ExchangeState.Exit)
                .Permit(ExchangeTrigger.Error, ExchangeState.Error)
                .PermitDynamic(ExchangeTrigger.Previous, () => _history.Count > 0 ? _history.Peek() : ExchangeState.Exit);

            // IDScanning (스캔 중)
            _fsm.Configure(ExchangeState.IDScanning)
                .OnEntryAsync(async () =>
                {
                    await _nav.NavigateTo<ExchangeIDScanningViewModel>(vm =>
                    {
                        vm.OnStepMain = async () => await ExitAsync();
                        vm.OnStepNext = async (bool? res) => await NextAsync();
                        vm.OnStepError = async ex =>
                        {
                            Debug.WriteLine(ex, "[ExchangeSellStateMachine] 신분증 스캔 중 오류 발생");
                            await ErrorAsync();
                        };
                    });
                })
                .Permit(ExchangeTrigger.Next, ExchangeState.IDScanningComplete)
                .Permit(ExchangeTrigger.Exit, ExchangeState.Exit)
                .Permit(ExchangeTrigger.Error, ExchangeState.Error)
                .PermitDynamic(ExchangeTrigger.Previous, () => _history.Count > 0 ? _history.Peek() : ExchangeState.Exit);

            // IDScanningComplete (검증/확인 후 입금 단계로)
            _fsm.Configure(ExchangeState.IDScanningComplete)
                .OnEntryAsync(async () =>
                {
                    // 예: 검증 결과 표시 후 다음(입금)으로 이동
                    await _nav.NavigateTo<ExchangeDepositViewModel>(vm =>
                    {
                        vm.OnStepMain = async () => await ExitAsync();
                        vm.OnStepPrevious = async () => await PreviousAsync();
                        vm.OnStepNext = async (bool? res) => await NextAsync();
                        vm.OnStepError = async ex =>
                        {
                            Debug.WriteLine(ex, "[ExchangeSellStateMachine] 신분증 스캔 완료 오류 발생");
                            await ErrorAsync();
                        };
                    });
                })
                .Permit(ExchangeTrigger.Next, ExchangeState.Deposit)
                .Permit(ExchangeTrigger.Exit, ExchangeState.Exit)
                .Permit(ExchangeTrigger.Error, ExchangeState.Error)
                .PermitDynamic(ExchangeTrigger.Previous, () => _history.Count > 0 ? _history.Peek() : ExchangeState.Exit);

            // Deposit (입금)
            _fsm.Configure(ExchangeState.Deposit)
                .OnEntryAsync(async () =>
                {
                    await _nav.NavigateTo<ExchangeDepositViewModel>(vm =>
                    {
                        vm.OnStepMain = async () => await ExitAsync();
                        vm.OnStepPrevious = async () => await PreviousAsync();
                        vm.OnStepNext = async (bool? res) => 
                        {
                            // 입금이 완료되면 서버 요청 단계로 진행
                            await NextAsync();
                        };
                        vm.OnStepError = async ex =>
                        {
                            Debug.WriteLine(ex, "[ExchangeSellStateMachine] 입금 화면 오류 발생");
                            await ErrorAsync();
                        };
                    });
                })
                .Permit(ExchangeTrigger.Next, ExchangeState.ApiRequest)
                .Permit(ExchangeTrigger.Exit, ExchangeState.Exit)
                .Permit(ExchangeTrigger.Error, ExchangeState.Error)
                .PermitDynamic(ExchangeTrigger.Previous, () => _history.Count > 0 ? _history.Peek() : ExchangeState.Exit);

            // ApiRequest (서버 요청 — 성공이면 Complete로, 실패면 Error로)
            _fsm.Configure(ExchangeState.ApiRequest)
                .OnEntryAsync(async () =>
                {
                    try
                    {
                        // 실제 API 호출을 여기서 수행하고, 결과에 따라 Next 또는 ErrorOccurred 호출
                        var ok = await CallExchangeApiAsync();
                        if (ok) await NextAsync(); // Api 성공 -> Complete
                        else await ErrorAsync();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex, "[ExchangeSellStateMachine] API 요청 중 예외");
                        await ErrorAsync();
                    }
                })
                .Permit(ExchangeTrigger.Next, ExchangeState.Complete)
                .Permit(ExchangeTrigger.Exit, ExchangeState.Exit)
                .Permit(ExchangeTrigger.Error, ExchangeState.Error)
                .PermitDynamic(ExchangeTrigger.Previous, () => _history.Count > 0 ? _history.Peek() : ExchangeState.Exit);

            // Complete 화면
            _fsm.Configure(ExchangeState.Complete)
                .OnEntryAsync(async () =>
                {
                    await _nav.NavigateTo<ExchangeCompleteViewModel>(vm =>
                    {

                    });
                })
                .Permit(ExchangeTrigger.Exit, ExchangeState.Exit);

            // Error 화면
            _fsm.Configure(ExchangeState.Error)
                .OnEntryAsync(async () =>
                {
                    Debug.WriteLine("Error Occured");
                })
                .Permit(ExchangeTrigger.Exit, ExchangeState.Exit);

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
