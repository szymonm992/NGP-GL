using Zenject;
using UnityEngine;
using TMPro;
using GLShared.General.Signals;
using GLShared.General.Interfaces;

namespace Frontend.Scripts
{
    public class BattleTimer : MonoBehaviour, IInitializable
    {
        [Inject] private readonly SignalBus signalBus;
        [Inject] private readonly IBattleParameters battleParameters;
        [Inject(Id = "battleTimer")] private readonly TextMeshProUGUI timerText;

        public void Initialize()
        {
            string seconds = GetSecondsString((int)battleParameters.BattleDurationTime % 60);
            timerText.text = $"{battleParameters.BattleDurationTime/60}:{seconds}";
            signalBus.Subscribe<PlayerSignals.OnBattleTimeChanged>(OnBattleTimeChanged);
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
