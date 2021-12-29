using Frontend.Scripts.Components;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Frontend.Scripts
{
    public class StateChanger : MonoBehaviour
    {
        [Inject] private readonly GameStateManager gameStateManager;
      
        void Start()
        {
            gameStateManager.ChangeState(GameState.Welcome);
        }

        private void Update()
        {
            if (gameStateManager.CurrentGameState == GameState.Welcome && !gameStateManager.IsChangingState)
            {
                Debug.Log("changing to lobby");
                gameStateManager.ChangeState(GameState.Lobby);
            }
              
        }
    }
}
