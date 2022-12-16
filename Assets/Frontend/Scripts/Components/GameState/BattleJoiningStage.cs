using Automachine.Scripts.Components;
using Frontend.Scripts.Enums;
using Frontend.Scripts.Models;
using Frontend.Scripts.Signals;
using GLShared.General.ScriptableObjects;
using Sfs2X.Entities.Data;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Zenject;
using static Frontend.Scripts.Signals.ConnectionSignals;

namespace Frontend.Scripts.Components.GameState
{
    public class BattleJoiningStage : State<WelcomeStage>
    {
        [Inject] private GameParameters gameParameters;
        [Inject] private ConnectionManager connectionManager;

        [Inject(Id = "joinBattleBtn")] private readonly Button joinBattleBtn;
        [Inject(Id = "countdownWaitingForBattle")] private readonly TextMeshProUGUI countdownText;
        [Inject(Id = "canvasSearchingBattle")] private readonly RectTransform joinBattleCanvas;

        private float battleSearchingTimer = 0;

        public bool IsTryingToJoinBattle { get; set; }

        public override void Initialize()
        {
            base.Initialize();
            signalBus.Subscribe<ConnectionSignals.OnRoomJoinResponse>(OnRoomJoinResponse);
        }

        public override void StartState()
        {
            base.StartState();

            battleSearchingTimer = 0;
            joinBattleBtn.interactable = false;
            joinBattleCanvas.gameObject.ToggleGameObjectIfActive(true);

            SendJoinBattleRequest();
        }

        public void TryJoinBattle()
        {
            if(IsTryingToJoinBattle)
            {
                return;
            }

            IsTryingToJoinBattle = true;
        }

        public override void Tick()
        {
            base.Tick();
            if(isActive && IsTryingToJoinBattle)
            {
                battleSearchingTimer += Time.deltaTime;
                countdownText.text = battleSearchingTimer.ToString("F2");
            }
        }

        public void FinishJoiningBattle(bool foundBattle = false)
        {
            IsTryingToJoinBattle = false;
        }

        private void OnRoomJoinResponse(OnRoomJoinResponse OnRoomJoinResponse)
        {
            Debug.Log("Battle join response: "+OnRoomJoinResponse.SuccessfullyJoinedRoom);

            if(OnRoomJoinResponse.SuccessfullyJoinedRoom)
            {
                StartCoroutine(LaunchGameScene());
            }
            else
            {
                FinishJoiningBattle();
            }
        }

        private IEnumerator LaunchGameScene()
        {
            var asyncOperation = SceneManager.LoadSceneAsync("MainTest");

            while(!asyncOperation.isDone)
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
            data.PutUtfString("playerVehicle", "T-55");
            data.PutUtfString("battleType", "randomBattle");

            connectionManager.SendRequest("startBattle", data);
        }
    }
}
