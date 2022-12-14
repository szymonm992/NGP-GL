
using Automachine.Scripts.Attributes;
using Frontend.Scripts.Components.GameState;

namespace GLShared.General.Enums
{
    [AutomachineStates]
    public enum WelcomeStage
    {
        [DefaultState, StateEntity(typeof(WelcomeNoneStage))]
        None = 0,

        [StateEntity(typeof(WelcomeOnLogin))]
        OnLoginAttempt = 1,

        OnLobbyJoining = 2,
    }
}
