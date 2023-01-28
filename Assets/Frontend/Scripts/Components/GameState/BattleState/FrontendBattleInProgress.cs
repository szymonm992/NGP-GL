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
            string minutes = GetTimeString((int)battleParameters.BattleDurationTime / 60);
            string seconds = GetTimeString((int)battleParameters.BattleDurationTime % 60);
            timerText.text = $"{minutes}:{seconds}";

            signalBus.Subscribe<PlayerSignals.OnBattleTimeChanged>(OnBattleTimeChanged);
        }

        public override void StartState()
        {
            base.StartState();

            StartCoroutine(HideBeginningText(3.0f));
        }

        private IEnumerator HideBeginningText(float delay)
        {
            yield return new WaitForSeconds(delay);
            countdownText.text = string.Empty;
        }

        private void OnBattleTimeChanged(PlayerSignals.OnBattleTimeChanged OnBattleTimeChanged)
        {
            string minutes = GetTimeString(OnBattleTimeChanged.CurrentMinutesLeft);
            string seconds = GetTimeString(OnBattleTimeChanged.CurrentSecondsLeft);
            timerText.text = $"{minutes}:{seconds}";
        }

        private string GetTimeString(int time)
        {
            if (time < 10)
            {
                return $"0{time}";
            }
            return time.ToString();
        }
    }
}
