using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Frontend.Scripts.Components
{
    public class GameStateManager : MonoBehaviour
    {
        private GameState currentGameState;
        private GameState previousGameState;

        private GameStateFactory gameStateFactory;
        private GameStateEntity gameStateEntity = null;

        public GameState CurrentGameState => currentGameState;
        public GameState PeeviousGameState => previousGameState;

        [Inject]
        public void Construct(GameStateFactory localGameStateFactory) => gameStateFactory = localGameStateFactory;

        private void Start() => ChangeState(GameState.Calibration);

        public void ChangeState(GameState gameState)
        {
            if (gameStateEntity != null)
            {
                gameStateEntity.Dispose();
                gameStateEntity = null;
            }

            previousGameState = currentGameState;

            currentGameState = gameState;

            gameStateEntity = gameStateFactory.CreateState(gameState);
            gameStateEntity.Start();
        }

    }
}
