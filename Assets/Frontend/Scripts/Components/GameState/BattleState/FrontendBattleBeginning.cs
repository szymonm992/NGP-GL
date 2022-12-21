using Automachine.Scripts.Components;
using Frontend.Scripts.Enums;
using GLShared.General.Interfaces;
using TMPro;
using UnityEngine;
using Zenject;

namespace Frontend.Scripts.Components.GameState
{
    public class FrontendBattleBeginning : State<FrontendBattleState>
    {
        [Inject] private readonly ISyncManager syncManager;
        [Inject(Id = "countdownText")] private readonly TextMeshProUGUI countdownText;

        public override void Initialize()
        {
            base.Initialize();
        }

        public override void StartState()
        {
            base.StartState();
            countdownText.text = "Initializing game...";
        }

        //syncManager.CreatePlayer(true, "T-55", new Vector3(132.35f, 2f, 118.99f), Quaternion.Euler(0, 90f, 0));
    }
}
