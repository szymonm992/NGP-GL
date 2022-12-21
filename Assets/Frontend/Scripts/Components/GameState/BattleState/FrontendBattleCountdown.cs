using Automachine.Scripts.Components;
using Frontend.Scripts.Enums;
using Frontend.Scripts.Signals;
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

        public override void Initialize()
        {
            base.Initialize();
            signalBus.Subscribe<BattleSignals.OnCounterUpdate>(OnCounterUpdate);
        }

        public void OnCounterUpdate(BattleSignals.OnCounterUpdate OnCounterUpdate)
        {
            int currentVal = OnCounterUpdate.CurrentValue;

            if (currentVal > 0)
            {
                countdownText.text = currentVal.ToString();
            }
            else
            {
                countdownText.text = "Let's go!";
            }
        }
    }
}
