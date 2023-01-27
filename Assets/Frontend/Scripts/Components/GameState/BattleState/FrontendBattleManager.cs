using Automachine.Scripts.Components;
using Automachine.Scripts.Signals;
using Frontend.Scripts.Components.GameState;
using Frontend.Scripts.Enums;
using Frontend.Scripts.ScriptableObjects;
using Frontend.Scripts.Signals;
using GLShared.General.Enums;
using GLShared.General.Interfaces;
using GLShared.General.ScriptableObjects;
using GLShared.General.Signals;
using GLShared.Networking.Components;
using Zenject;

namespace Frontend.Scripts.Components
{
    public class FrontendBattleManager : AutomachineEntity<FrontendBattleState>, IBattleManager
    {
        [Inject] private readonly ISyncManager syncManager;
        [Inject] private readonly RandomBattleParameters battleParameters;
        [Inject] private readonly SmartFoxConnection smartFox;
        [Inject] private readonly FrontSettings frontSettings;

        private FrontendBattleCountdown countdownState;
        private FrontendBattleInProgress battleInProgressState;

        private BattleStage currentBattleStage;

        public BattleStage CurrentBattleStage => currentBattleStage;

        public override void Initialize()
        {
            base.Initialize();

            signalBus.Subscribe<BattleSignals.OnGameStageUpdate>(OnGameStageUpdate);
            signalBus.Subscribe<OnStateEnter<FrontendBattleState>>(OnStateEnter);

            smartFox.Connection.InitUDP(frontSettings.ConnectLocalhost ? frontSettings.LocalhostAddress : frontSettings.WanAddress, smartFox.PORT);
        }

        public override void OnStateMachineInitialized(OnStateMachineInitialized<FrontendBattleState> OnStateMachineInitialized)
        {
            base.OnStateMachineInitialized(OnStateMachineInitialized);

            countdownState = (FrontendBattleCountdown)stateMachine.GetState(FrontendBattleState.Countdown);
            battleInProgressState = (FrontendBattleInProgress)stateMachine.GetState(FrontendBattleState.InProgress);

            stateMachine.AddTransition(FrontendBattleState.OnBeginning, FrontendBattleState.Countdown,
                () => currentBattleStage == BattleStage.Countdown);
            
            stateMachine.AddTransition(FrontendBattleState.Countdown, FrontendBattleState.InProgress,
                () => currentBattleStage == BattleStage.InProgress);

            stateMachine.AddTransition(FrontendBattleState.InProgress, FrontendBattleState.Ending,
                () => currentBattleStage == BattleStage.Ending);
        }

        public void OnStateEnter(OnStateEnter<FrontendBattleState> OnStateEnter)
        {
            bool lockPlayerInput = OnStateEnter.signalStateStarted != FrontendBattleState.InProgress;
            signalBus.Fire(new PlayerSignals.OnAllPlayersInputLockUpdate()
            {
                LockPlayersInput = lockPlayerInput
            });
        }

        private void OnGameStageUpdate(BattleSignals.OnGameStageUpdate OnGameStageUpdate)
        {
            int currentStageIndex = OnGameStageUpdate.CurrentGameStage;
            currentBattleStage = (BattleStage)currentStageIndex;
        }
    }
}
