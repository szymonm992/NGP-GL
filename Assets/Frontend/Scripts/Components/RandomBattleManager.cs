using Automachine.Scripts.Components;
using Automachine.Scripts.Signals;
using GLShared.General.Components;
using GLShared.General.Enums;
using GLShared.General.Interfaces;
using GLShared.General.ScriptableObjects;
using GLShared.General.Signals;
using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Frontend.Scripts.Components
{
    public class RandomBattleManager : AutomachineEntity<BattleStage>
    {
        [Inject] private readonly ISyncManager syncManager;
        [Inject] private readonly RandomBattleParameters battleParameters;

        

        private BattleCountdownStage countdownState;
        private BattleBeginningStage beginningStage;

        private bool allPlayersInitialized = false;
        private bool allPlayersConnectionsEstablished = false;

        public override void OnStateMachineInitialized(OnStateMachineInitialized<BattleStage> OnStateMachineInitialized)
        {
            base.OnStateMachineInitialized(OnStateMachineInitialized);

            beginningStage = (BattleBeginningStage)stateMachine.GetState(BattleStage.Beginning);
            countdownState = (BattleCountdownStage)stateMachine.GetState(BattleStage.Countdown);

            stateMachine.AddTransition(BattleStage.Beginning, BattleStage.Countdown
                , () => battleParameters.AreAllPlayersSpawned.Invoke(syncManager.SpawnedPlayersAmount)
                && allPlayersConnectionsEstablished && allPlayersInitialized, 2f);

            stateMachine.AddTransition(BattleStage.Countdown, BattleStage.InProgress,
                () => countdownState.FinishedCountdown);

            signalBus.Subscribe<OnStateEnter<BattleStage>>(OnStateEnter);
        }

        public void OnStateEnter(OnStateEnter<BattleStage> OnStateEnter)
        {
            bool lockPlayerInput = OnStateEnter.signalStateStarted != BattleStage.InProgress;
            signalBus.Fire(new PlayerSignals.OnAllPlayersInputLockUpdate()
            {
                LockPlayersInput = lockPlayerInput
            });
        }
    }
}
