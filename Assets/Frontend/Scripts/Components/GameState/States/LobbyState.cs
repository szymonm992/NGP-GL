using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
using UnityEngine.Scripting;
namespace Frontend.Scripts.Components
{
    public class LobbyState : GameStateEntity, IGameState
    {

        public override void Initialize()
        {
        }

        public override void Start()
        {
            Debug.Log("Lobby started...");
        }

        public override void Tick()
        {
        }

        public override void Dispose()
        {
        }

        public void Startt()
        {
            Debug.Log("Lobby started...");
        }


        public class Factory : PlaceholderFactory<LobbyState>
        {
        }
    }



}
