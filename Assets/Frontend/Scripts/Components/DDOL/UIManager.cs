using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

using Frontend.Scripts.Signals;

namespace Frontend.Scripts.Components
{
    public class UIManager : MonoBehaviour
    {
        [Inject] private readonly SignalBus signalBus;

        private void Start()
        {
           // signalBus.Subscribe<GameStateChangedSignal>(UpdateGameStateUI);
        }

        //private void UpdateGameStateUI(GameStateChangedSignal args)
        //{
         //   Debug.Log("State was changed to: "+args.gameState);
        //}
    }
}
