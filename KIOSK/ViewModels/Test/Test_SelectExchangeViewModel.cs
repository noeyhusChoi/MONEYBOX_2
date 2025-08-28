using CommunityToolkit.Mvvm.Input;
using KIOSK.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KIOSK.ViewModels.Test
{
    partial class Test_SelectExchangeViewModel
    {
        private readonly Test_StateMachineService _stateMachine;

        public Test_SelectExchangeViewModel(Test_StateMachineService stateMachine)
        {
            _stateMachine = stateMachine;
        }

        [RelayCommand]
        private void Okay()
        {
            _stateMachine.GoToState(Test_StateMachineService.PageState.Complete);
        }

        [RelayCommand]
        private void Cancel()
        {
            _stateMachine.GoToState(Test_StateMachineService.PageState.Terms);
        }
    }
}
