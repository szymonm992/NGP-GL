using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Frontend.Scripts.Components
{
    public class GameStateManager : IInitializable
    {
        private GameState currentGameState;
        private GameState previousGameState;

        private GameStateFactory gameStateFactory;
        private GameStateEntity gameStateEntity = null;

        public GameState CurrentGameState => currentGameState;
        public GameState PeeviousGameState => previousGameState;

        StateFactory[] stateFactory;
        IGameState[] strategy;
        public void Initialize()
        {
            ChangeState(GameState.Calibration);
        }

        public GameStateManager(StateFactory[] localStateFactory)
        {
            stateFactory = localStateFactory;
        }


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
           // _strategy = _strategyFactory.Create();
           // _strategy.Startt();
            //gameStateEntity = gameStateFactory.CreateState(gameState);
          //  gameStateEntity.Start();
        }

    }
}
