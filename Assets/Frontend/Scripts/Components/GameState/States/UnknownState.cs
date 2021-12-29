using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
using UnityEngine.Scripting;
namespace Frontend.Scripts.Components
{
    public class UnknownState : GameStateEntity, IGameState
    {
        public GameState ConnectedState { get; set; }

        public UnknownState(GameState st)
        {
            ConnectedState = st;
        }

        public void Start()
        {
            Debug.Log("Unknown state");
        }
    }
}
