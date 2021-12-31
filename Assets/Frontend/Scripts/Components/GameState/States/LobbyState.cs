using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
using UnityEngine.Scripting;
namespace Frontend.Scripts.Components
{
    public class LobbyState :  GameStateEntity, IGameState
    {
        public override GameState ConnectedState { get; set; }

        public LobbyState(GameState st)
        {
            ConnectedState = st;
        }

        public override void Start()
        {
            Debug.Log("Lobby started...");
        }
    }



}
