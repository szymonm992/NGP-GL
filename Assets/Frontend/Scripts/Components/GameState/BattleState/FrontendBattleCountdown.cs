using Automachine.Scripts.Components;
using Frontend.Scripts.Enums;
using GLShared.General.Interfaces;
using TMPro;
using UnityEngine;
using Zenject;

namespace Frontend.Scripts.Components.GameState
{
    public class FrontendBattleCountdown : State<FrontendBattleState>
    {
        [Inject] private readonly ISyncManager syncManager;
        [Inject(Id = "countdownText")] private readonly TextMeshProUGUI countdownText;

        public override void StartState()
        {
            base.StartState();
            countdownText.text = "Initializing game...";
        }

        public void SetCounter(int number)
        {
            if(number < 0)
            {
                countdownText.text = number.ToString();
            }
            else
            {
                countdownText.text = "Let's go!";
            }
        }

    }
}
