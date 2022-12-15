
using Automachine.Scripts.Attributes;
using Frontend.Scripts.Components.GameState;
using UnityEngine;

namespace Frontend.Scripts.Enums
{
    [AutomachineStates]
    public enum WelcomeStage
    {
        [DefaultState, StateEntity(typeof(WelcomeNoneStage))]
        None = 0,

        [StateEntity(typeof(WelcomeOnLoginAttempt))]
        OnLoginAttempt = 1,

        [StateEntity(typeof(LobbyJoinedStage))]
        OnLobbyJoining = 2,

        [StateEntity(typeof(BattleJoiningStage))]
        OnBattleJoining = 3
    }
}
