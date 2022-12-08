using Frontend.Scripts.Signals;
using GLShared.General.Interfaces;
using GLShared.General.Signals;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
using static GLShared.General.Signals.PlayerSignals;

namespace Frontend.Scripts.Components.Temporary
{
    public class TempGameManager : MonoBehaviour, IInitializable
    {
        [Inject] private readonly SignalBus signalBus;

        public void Initialize()
        {
            
        }

        
    }
}
