using Automachine.Scripts.Components;
using Automachine.Scripts.Signals;
using GLShared.General.Components;
using GLShared.General.Enums;
using GLShared.General.ScriptableObjects;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Frontend.Scripts.Components
{
    public class RandomBattleManager : AutomachineEntity<BattleStage>
    {
        [SerializeField] private bool allPlayersSpawned = true;
        [SerializeField] private bool allPlayersConnectionsEstablished = true;

        private BattleCountdownStage countdownState;
        private BattleBeginningStage beginningStage;

        public override void OnStateMachineInitialized(OnStateMachineInitialized<BattleStage> OnStateMachineInitialized)
        {
            beginningStage = (BattleBeginningStage)stateMachine.GetState(BattleStage.Beginning);
            countdownState = (BattleCountdownStage)stateMachine.GetState(BattleStage.Countdown);
            
            base.OnStateMachineInitialized(OnStateMachineInitialized);
            stateMachine.AddTransition(BattleStage.Beginning, BattleStage.Countdown
                , () => allPlayersSpawned && allPlayersConnectionsEstablished);

            stateMachine.AddTransition(BattleStage.Countdown, BattleStage.InProgress, () => countdownState.FinishedCountdown);
        }
    }
}
