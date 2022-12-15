using Automachine.Scripts.Components;
using Frontend.Scripts.Enums;
using GLShared.General.ScriptableObjects;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Frontend.Scripts.Components.GameState
{
    public class BattleJoiningStage : State<WelcomeStage>
    {
        [Inject] private GameParameters gameParameters;
        [Inject(Id = "joinBattleBtn")] private readonly Button joinBattleBtn;
        [Inject(Id = "countdownWaitingForBattle")] private readonly TextMeshProUGUI countdownText;
        [Inject(Id = "canvasSearchingBattle")] private readonly RectTransform joinBattleCanvas;

        private float battleSearchingTimer = 0;

        public bool IsTryingToJoinBattle { get; set; }

        public override void StartState()
        {
            base.StartState();

            battleSearchingTimer = 0;
            joinBattleBtn.interactable = false;
            joinBattleCanvas.gameObject.ToggleGameObjectIfActive(true);
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
    }
}
