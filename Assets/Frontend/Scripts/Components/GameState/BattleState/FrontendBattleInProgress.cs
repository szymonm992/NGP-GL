using Automachine.Scripts.Components;
using Frontend.Scripts.Enums;
using GLShared.General.Interfaces;
using GLShared.General.Signals;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Zenject;

namespace Frontend.Scripts.Components.GameState
{
    public class FrontendBattleInProgress : State<FrontendBattleState>
    {
        [Inject] private readonly ISyncManager syncManager;
        [Inject] private readonly IBattleParameters battleParameters;
        [Inject(Id = "countdownText")] private readonly TextMeshProUGUI countdownText;
        [Inject(Id = "battleTimer")] private readonly TextMeshProUGUI timerText;

        public override void Initialize()
        {
            string seconds = GetSecondsString((int)battleParameters.BattleDurationTime % 60);
            timerText.text = $"{battleParameters.BattleDurationTime / 60}:{seconds}";
            signalBus.Subscribe<PlayerSignals.OnBattleTimeChanged>(OnBattleTimeChanged);
        }

        public override void StartState()
        {
            base.StartState();
            Debug.Log("The battle just started!");
            StartCoroutine(HideBeginningText(3f));
        }

        private IEnumerator HideBeginningText(float delay)
        {
            yield return new WaitForSeconds(delay);
            countdownText.text = "";
        }

        private void OnBattleTimeChanged(PlayerSignals.OnBattleTimeChanged OnBattleTimeChanged)
        {
            string seconds = GetSecondsString(OnBattleTimeChanged.CurrentSecondsLeft);
            timerText.text = $"{OnBattleTimeChanged.CurrentMinutesLeft}:{seconds}";
        }

        private string GetSecondsString(int seconds)
        {
            if (seconds < 10)
            {
                return "0" + seconds;
            }
            return seconds.ToString();
        }
    }
}
