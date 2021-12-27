using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
using UnityEngine.Scripting;
namespace Frontend.Scripts.Components
{
    public class CalibrationState : GameStateEntity,IGameState
    {
        
        public override void Initialize()
        {
        }

        public override void Start()
        {
            Debug.Log("Calibration started...");
        }

        public override void Tick()
        {
        }

        public override void Dispose()
        {
        }

        public class Factory : PlaceholderFactory<CalibrationState>
        {
        }
    }
  



}
