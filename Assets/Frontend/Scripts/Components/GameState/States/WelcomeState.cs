using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
using UnityEngine.Scripting;
namespace Frontend.Scripts.Components
{
    public class WelcomeState : GameStateEntity, IGameState
    {
        public GameState ConnectedState { get; set; }

        public WelcomeState(GameState st)
        {
            ConnectedState = st;
        }
        
        public void Start()
        {
            Debug.Log("Welcome started...");
        }
    }
}
