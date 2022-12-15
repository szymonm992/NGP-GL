using Automachine.Scripts.Components;
using Frontend.Scripts.Enums;
using Frontend.Scripts.Models;
using TMPro;
using UnityEngine;
using Zenject;

namespace Frontend.Scripts.Components.GameState
{
    public class WelcomeNoneStage : State<WelcomeStage>
    {
        [Inject(Id = "errorLabel")] private TextMeshProUGUI errorLabel;
        [Inject(Id = "welcomeCanvas")] private readonly RectTransform welcomeUi;

        public override void Initialize()
        {
            base.Initialize();
            errorLabel.text = string.Empty;
        }

        public override void StartState()
        {
            base.StartState();
            welcomeUi.gameObject.ToggleGameObjectIfActive(true);
        }
    }
}
