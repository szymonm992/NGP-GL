using Automachine.Scripts.Components;
using Automachine.Scripts.Signals;
using Frontend.Scripts.Enums;
using GLShared.General.Components;
using GLShared.General.Enums;
using GLShared.General.Interfaces;
using GLShared.General.ScriptableObjects;
using GLShared.General.Signals;
using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Frontend.Scripts.Components
{
    public class WelcomeManager : AutomachineEntity<WelcomeStage>
    {
        public override void OnStateMachineInitialized(OnStateMachineInitialized<WelcomeStage> OnStateMachineInitialized)
        {
            base.OnStateMachineInitialized(OnStateMachineInitialized);
        }
    }
}
