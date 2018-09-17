// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using System.Windows.Input;
using Stateless;

namespace SnipInsight.StateMachine
{
    public static class StateMachineExtensions
    {
        public static ICommand CreateCommand<TState, TTrigger>(this StateMachine<TState, TTrigger> stateMachine,
            TTrigger trigger)
        {
            return new RelayCommand
                (
                () => stateMachine.Fire(trigger),
                () => stateMachine.CanFire(trigger)
                );
        }
    }
}
