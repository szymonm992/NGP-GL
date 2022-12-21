using Automachine.Scripts.Components;
using Frontend.Scripts.Enums;
using GLShared.General.Interfaces;
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
        [Inject(Id = "countdownText")] private readonly TextMeshProUGUI countdownText;

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
    }
}
