using Automachine.Scripts.Components;
using Automachine.Scripts.Signals;
using Frontend.Scripts.Enums;
using GLShared.General.Components;
using GLShared.General.Enums;
using GLShared.General.Interfaces;
using GLShared.General.ScriptableObjects;
using GLShared.General.Signals;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Frontend.Scripts
{
    public class FrontendBattleManager : AutomachineEntity<FrontendBattleState>, IBattleManager
    {
        [Inject] private readonly ISyncManager syncManager;
        [Inject] private readonly RandomBattleParameters battleParameters;

        private BattleStage currentBattleStage;

        public override void OnStateMachineInitialized(OnStateMachineInitialized<FrontendBattleState> OnStateMachineInitialized)
        {
            base.OnStateMachineInitialized(OnStateMachineInitialized);

            stateMachine.AddTransition(FrontendBattleState.OnBeginning, FrontendBattleState.Countdown,
                () => currentBattleStage == BattleStage.Countdown);
            
            stateMachine.AddTransition(FrontendBattleState.Countdown, FrontendBattleState.InProgress,
                () => currentBattleStage == BattleStage.InProgress);

            stateMachine.AddTransition(FrontendBattleState.InProgress, FrontendBattleState.Ending,
                () => currentBattleStage == BattleStage.Ending);

            signalBus.Subscribe<OnStateEnter<FrontendBattleState>>(OnStateEnter);
        }

        public void OnStateEnter(OnStateEnter<FrontendBattleState> OnStateEnter)
        {
            bool lockPlayerInput = OnStateEnter.signalStateStarted != FrontendBattleState.InProgress;
            signalBus.Fire(new PlayerSignals.OnAllPlayersInputLockUpdate()
            {
                LockPlayersInput = lockPlayerInput
            });
        }

        public void ApplyCurrentState(BattleStage battleStage)
        {
            currentBattleStage = battleStage;
        }
    }
}
