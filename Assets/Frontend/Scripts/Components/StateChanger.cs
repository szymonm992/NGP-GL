using Frontend.Scripts.Components;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
using Frontend.Scripts.Enums;

namespace Frontend.Scripts
{
    public class StateChanger : MonoBehaviour
    {
        [Inject] private readonly GameStateManager gameStateManager;
        
        private void Start()
        {
            gameStateManager.ChangeState(GameState.Lobby);
        }


        private void Update()
        {
            /*
            if (gameStateManager.CurrentGameState == GameState.Welcome && !gameStateManager.IsChangingState)
            {
                gameStateManager.ChangeState(GameState.Lobby);  
            }*/
              
        }
    }
}
