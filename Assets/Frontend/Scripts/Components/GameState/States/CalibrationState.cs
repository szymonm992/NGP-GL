using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
using UnityEngine.Scripting;
namespace Frontend.Scripts.Components
{
    public class CalibrationState : GameStateEntity, IGameState
    {
        public GameState ConnectedState { get; set; }

        public CalibrationState(GameState st)
        {
            ConnectedState = st;
        }
        
        public void Start()
        {
            Debug.Log("Calibration started...");
        }
    }
}
