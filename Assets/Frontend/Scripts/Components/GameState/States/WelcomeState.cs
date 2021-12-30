using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
using UnityEngine.Scripting;
using System.Linq;

namespace Frontend.Scripts.Components
{
    public class WelcomeState : GameStateEntity, IGameState
    {
       
        [Inject] private readonly WelcomeManager manager;
        public GameState ConnectedState { get; set; }

        public WelcomeState(GameState st)
        {
            ConnectedState = st;
        }
        
        public void Start()
        {
            Debug.Log("Welcome started...");
          
            Debug.Log(manager==null);
        }
    }
}
