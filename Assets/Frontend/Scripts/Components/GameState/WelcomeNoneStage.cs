using Automachine.Scripts.Components;
using Frontend.Scripts.Enums;
using TMPro;
using Zenject;

namespace Frontend.Scripts.Components.GameState
{
    public class WelcomeNoneStage : State<WelcomeStage>
    {
        [Inject(Id = "errorLabel")] private TextMeshProUGUI errorLabel;

        public override void Initialize()
        {
            base.Initialize();
            errorLabel.text = string.Empty;
        }
    }
}
