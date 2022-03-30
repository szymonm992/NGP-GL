using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
using UnityEngine.Scripting;
using Frontend.Scripts.Enums;
using Frontend.Scripts.Interfaces;

namespace Frontend.Scripts.Components
{
    public class UnknownState : GameStateEntity, IGameState
    {
        public override GameState ConnectedState { get; set; }

        public UnknownState(GameState st)
        {
            ConnectedState = st;
        }
        public override void Start()
        {
            Debug.Log("Unknown state");
        }
    }
}
