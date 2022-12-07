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
        [Inject] private readonly RandomBattleParameters randomBattleParameters;

        [SerializeField] private bool allPlayersSpawned;
        [SerializeField] private bool allPlayersConnectionsEstablished;

        private BattleCountdownStage countdownState;

        public override void OnStateMachineInitialized(OnStateMachineInitialized<BattleStage> OnStateMachineInitialized)
        {
            countdownState = (BattleCountdownStage)stateMachine.GetState(BattleStage.Countdown);
            base.OnStateMachineInitialized(OnStateMachineInitialized);
            stateMachine.AddTransition(BattleStage.Beginning, BattleStage.Countdown
                , () => allPlayersSpawned && allPlayersConnectionsEstablished);

            stateMachine.AddTransition(BattleStage.Countdown, BattleStage.InProgress, () => countdownState.FinishedCountdown);

        }
    }
}
