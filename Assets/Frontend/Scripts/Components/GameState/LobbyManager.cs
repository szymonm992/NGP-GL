using Automachine.Scripts.Components;
using Automachine.Scripts.Signals;
using Frontend.Scripts.Components.GameState;
using Frontend.Scripts.Enums;
using GLShared.General.Enums;
using Sfs2X.Core;
using Sfs2X;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using GLShared.Networking.Components;
using Frontend.Scripts.Signals;

namespace Frontend.Scripts.Components
{
    public class LobbyManager : AutomachineEntity<WelcomeStage>
    {
        [Inject] private readonly SmartFoxConnection smartFox;
        [Inject] private readonly ConnectionManager connectionManager;

        private WelcomeOnLoginAttempt onLoginState;
        private BattleJoiningStage battleJoiningStage;
        private string disconnectionReason;

        public override void Initialize()
        {
            base.Initialize();
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }

        public override void OnStateMachineInitialized(OnStateMachineInitialized<WelcomeStage> OnStateMachineInitialized)
        {
            base.OnStateMachineInitialized(OnStateMachineInitialized);
            onLoginState = (WelcomeOnLoginAttempt)stateMachine.GetState(WelcomeStage.OnLoginAttempt);
            battleJoiningStage = (BattleJoiningStage)stateMachine.GetState(WelcomeStage.OnBattleJoining);

            stateMachine.AddTransition(WelcomeStage.None, WelcomeStage.OnLoginAttempt, () => onLoginState.IsTryingToLogin);

            stateMachine.AddTransition(WelcomeStage.OnLoginAttempt, WelcomeStage.None,
               () => !onLoginState.IsTryingToLogin && onLoginState.TriedToLogin && !onLoginState.LoginResult);

            stateMachine.AddTransition(WelcomeStage.OnLoginAttempt, WelcomeStage.OnLobbyJoining,
               () => !onLoginState.IsTryingToLogin && onLoginState.TriedToLogin && onLoginState.LoginResult);

            stateMachine.AddTransition(WelcomeStage.OnLobbyJoining, WelcomeStage.OnBattleJoining,
                () => onLoginState.LoginResult && battleJoiningStage.IsTryingToJoinBattle);

            stateMachine.AddTransition(WelcomeStage.OnBattleJoining, WelcomeStage.OnLobbyJoining,
               () => onLoginState.LoginResult && !battleJoiningStage.IsTryingToJoinBattle);

            signalBus.Subscribe<OnStateEnter<WelcomeStage>>(OnStateEnter);
            signalBus.Subscribe<ConnectionSignals.OnDisconnectedFromServer>(OnDisconnected);
        }

        public void OnStateEnter(OnStateEnter<WelcomeStage> OnStateEnter)
        {
            var state = OnStateEnter.signalStateStarted;

            if (state == WelcomeStage.None)
            {
                onLoginState.TriedToLogin = false;

                if (disconnectionReason != string.Empty)
                {
                    onLoginState.DisplayError(disconnectionReason);
                    disconnectionReason = string.Empty;
                }
            }
        }
        
        public void TryLogin()
        {
            onLoginState.TryLogin();
        }

        public void TryJoinBattle()
        {
            battleJoiningStage.TryJoinBattle();
        }

        public void TryCancelSearchingForBattle()
        {
            connectionManager.SendRequest("joiningBattle.cancelJoiningBattle");
        }

        public void OnDisconnected(ConnectionSignals.OnDisconnectedFromServer OnDisconnected)
        {
            disconnectionReason = OnDisconnected.Reason;
            stateMachine.ChangeState(WelcomeStage.None);
        }

        private void OnApplicationQuit()
        {
            TryCancelSearchingForBattle();
        }

        public void QuitGame()
        {
            Application.Quit();
        }
    }
}
