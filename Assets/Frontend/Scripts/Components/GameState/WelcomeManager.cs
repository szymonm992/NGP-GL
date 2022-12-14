using Automachine.Scripts.Components;
using Automachine.Scripts.Signals;
using Frontend.Scripts.Components.GameState;
using Frontend.Scripts.Enums;
using GLShared.General.Enums;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Frontend.Scripts.Components
{
    public class WelcomeManager : AutomachineEntity<WelcomeStage>
    {

        private WelcomeOnLoginAttempt onLoginState;

        public override void OnStateMachineInitialized(OnStateMachineInitialized<WelcomeStage> OnStateMachineInitialized)
        {
            base.OnStateMachineInitialized(OnStateMachineInitialized);
            onLoginState = (WelcomeOnLoginAttempt)stateMachine.GetState(WelcomeStage.OnLoginAttempt);

            stateMachine.AddTransition(WelcomeStage.None, WelcomeStage.OnLoginAttempt, () => onLoginState.IsTryingToLogin);

            stateMachine.AddTransition(WelcomeStage.OnLoginAttempt, WelcomeStage.OnLobbyJoining,
               () => !onLoginState.IsTryingToLogin && onLoginState.TriedToLogin && onLoginState.LoginResult);

            stateMachine.AddTransition(WelcomeStage.OnLoginAttempt, WelcomeStage.None, 
                () => !onLoginState.IsTryingToLogin && onLoginState.TriedToLogin && !onLoginState.LoginResult);

            signalBus.Subscribe<OnStateEnter<WelcomeStage>>(OnStateEnter);
        }

        public void OnStateEnter(OnStateEnter<WelcomeStage> OnStateEnter)
        {
            var state = OnStateEnter.signalStateStarted;
            if(state == WelcomeStage.None)
            {
                onLoginState.TriedToLogin = false;
            }
        }

        public void TryLogin()
        {
            onLoginState.TryLogin();
        }
    }
}
