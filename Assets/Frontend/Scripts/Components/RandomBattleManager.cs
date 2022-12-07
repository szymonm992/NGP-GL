using Automachine.Scripts.Components;
using Automachine.Scripts.Signals;
using GLShared.General.Enums;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Frontend.Scripts.Components
{
    public class RandomBattleManager : AutomachineEntity<BattleStage>
    {
        public override void OnStateMachineInitialized(OnStateMachineInitialized<BattleStage> OnStateMachineInitialized)
        {
            base.OnStateMachineInitialized(OnStateMachineInitialized);
        }
    }
}
