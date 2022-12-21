using Automachine.Scripts.Attributes;
using Frontend.Scripts.Components.GameState;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Frontend.Scripts.Enums
{
    [AutomachineStates]
    public enum FrontendBattleState 
    {
        [DefaultState, StateEntity(typeof(FrontendBattleBeginning))]
        OnBeginning = 0,
        [StateEntity(typeof(FrontendBattleCountdown))]
        Countdown = 1,
        [StateEntity(typeof(FrontendBattleInProgress))]
        InProgress,
        Ending,

    }
}
