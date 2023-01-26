using Automachine.Scripts.Components;
using Frontend.Scripts.Enums;
using Frontend.Scripts.Signals;
using GLShared.General.ScriptableObjects;
using Sfs2X.Entities.Data;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Zenject;

namespace Frontend.Scripts.Components.GameState
{
    public class BattleJoiningStage : State<WelcomeStage>
    {
        private const string VAR_BATTLE_TYPE = "battleType";
        private const string VAR_PLAYER_VEHICLE = "playerVehicle";

        [Inject] private readonly GameParameters gameParameters;
        [Inject] private readonly ConnectionManager connectionManager;

        [Inject(Id = "joinBattleBtn")] private readonly Button joinBattleBtn;
        [Inject(Id = "countdownWaitingForBattle")] private readonly TextMeshProUGUI countdownText;
        [Inject(Id = "countdownPlayersAmount")] private readonly TextMeshProUGUI playersInQueueAmount;
        [Inject(Id = "canvasSearchingBattle")] private readonly RectTransform joinBattleCanvas;

        private float battleSearchingTimer = 0;

        public bool IsTryingToJoinBattle { get; set; }

        public override void Initialize()
        {
            base.Initialize();
            signalBus.Subscribe<ConnectionSignals.OnRoomJoinResponse>(OnRoomJoinResponse);
            signalBus.Subscribe<ConnectionSignals.OnCancelEnteringBattle>(OnCancelEnteringBattle);
            signalBus.Subscribe<ConnectionSignals.OnDisconnectedFromServer>(OnDisconnectedFromServer);
            signalBus.Subscribe<ConnectionSignals.OnBattleJoiningInfoReceived>(OnBattleJoiningInfoReceived);
        }

        public override void StartState()
        {
            base.StartState();

            FillInWaitingForm(null);
            battleSearchingTimer = 0;
            joinBattleBtn.interactable = false;
            joinBattleCanvas.gameObject.ToggleGameObjectIfActive(true);

            SendJoinBattleRequest();
        }

        public void TryJoinBattle()
        {
            if (IsTryingToJoinBattle)
            {
                return;
            }

            IsTryingToJoinBattle = true;
        }

        public override void Tick()
        {
            base.Tick();
            if (isActive && IsTryingToJoinBattle)
            {
                battleSearchingTimer += Time.deltaTime;
                countdownText.text = battleSearchingTimer.ToString("F2");
            }
        }

        public void FinishJoiningBattle(bool foundBattle = false)
        {
            IsTryingToJoinBattle = false;
        }

        public void OnCancelEnteringBattle(ConnectionSignals.OnCancelEnteringBattle OnCancelEnteringBattle)
        {
            if(OnCancelEnteringBattle.SuccessfullyCanceled)
            {
                FinishJoiningBattle();
            }
        }

        public void OnDisconnectedFromServer(ConnectionSignals.OnDisconnectedFromServer OnDisconnectedFromServer)
        {
            FinishJoiningBattle();
            StopAllCoroutines();
        }

        private void OnRoomJoinResponse(ConnectionSignals.OnRoomJoinResponse OnRoomJoinResponse)
        {
            Debug.Log("Battle join response: "+OnRoomJoinResponse.SuccessfullyJoinedRoom);

            if (OnRoomJoinResponse.SuccessfullyJoinedRoom)
            {
                StartCoroutine(LaunchGameScene());
            }
            else
            {
                FinishJoiningBattle();
            }
        }

        private void OnBattleJoiningInfoReceived(ConnectionSignals.OnBattleJoiningInfoReceived OnBattleJoiningInfoReceived)
        {
            if (IsTryingToJoinBattle)
            {
                FillInWaitingForm(OnBattleJoiningInfoReceived.BattleJoiningInfoData);
            }
        }

        private void FillInWaitingForm(ISFSObject battleJoiningInfo = null)
        {
            if (battleJoiningInfo == null)
            {
                playersInQueueAmount.text = $"Loading players in queue...";
            }
            else
            {
                int playersInPool = battleJoiningInfo.GetInt("playersAmountInQueue");
                playersInQueueAmount.text = $"Players: {playersInPool}";
            }
        }

        private IEnumerator LaunchGameScene()
        {
            var asyncOperation = SceneManager.LoadSceneAsync("MainTest");

            while (!asyncOperation.isDone)
            {
                battleSearchingTimer += Time.deltaTime;
                countdownText.text = battleSearchingTimer.ToString("F2");
                yield return null;
            }

            Debug.Log("Successfully joined battle after: " + battleSearchingTimer);
            FinishJoiningBattle(true);
        }

        private void SendJoinBattleRequest()
        {
            ISFSObject data = new SFSObject();

            data.PutUtfString(VAR_PLAYER_VEHICLE, "T-55");
            data.PutUtfString(VAR_BATTLE_TYPE, "randomBattle");

            connectionManager.SendRequest("joiningBattle.startBattle", data);
        }
    }
}
