using Automachine.Scripts.Components;
using Frontend.Scripts.Enums;
using GLShared.Networking.Components;
using TMPro;
using UnityEngine;
using Zenject;

namespace Frontend.Scripts.Components.GameState
{
    public class WelcomeNoneStage : State<WelcomeStage>
    {
        [Inject] private readonly SmartFoxConnection smartFoxConnection;
        [Inject(Id = "errorLabel")] private TextMeshProUGUI errorLabel;
        [Inject(Id = "welcomeCanvas")] private readonly RectTransform welcomeUi;

        public override void Initialize()
        {
            base.Initialize();

            if (smartFoxConnection.DisconnectError != string.Empty)
            {
                errorLabel.text = smartFoxConnection.DisconnectError;
            }
            else
            {
                errorLabel.text = string.Empty;
            }
        }

        public override void StartState()
        {
            base.StartState();
            welcomeUi.gameObject.ToggleGameObjectIfActive(true);
        }
    }
}
