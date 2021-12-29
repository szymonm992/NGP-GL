using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
using System.Linq;
using Frontend.Scripts.Models;

namespace Frontend.Scripts.Components
{
    public class GameStateManager
    {
        [Inject] private readonly AsyncProcessor asyncProcessor;

        private readonly Dictionary<IGameState, StateFactory> allStates = new Dictionary<IGameState, StateFactory>();
        private readonly DiContainer diContainer;

        private GameState currentGameState;
        private GameState previousGameState;

        private GameStateEntity gameStateEntity = null;

        public GameState CurrentGameState => currentGameState;
        public GameState PeeviousGameState => previousGameState;

        public bool IsChangingState { get; private set; }

        public GameStateManager(DiContainer localDiContainer, StateFactory[] localStateFactory,IGameState[] localAllStates)
        {
            diContainer = localDiContainer;
            for (int i=0; i< localAllStates.Length;i++)
            {
                allStates.Add(localAllStates[i], localStateFactory[i]);
            }
        }

        public void ChangeStateDelayed(GameState gameState, float delayInSeconds)
        {
            asyncProcessor.StartNewCoroutine(()=> ChangeState(gameState), delayInSeconds);
        }

        public void ChangeState(GameState gameState)
        {
            if(IsChangingState)
            {
                Debug.LogError("Cannot change state while the state is being changed already");
                return;
            }

            IsChangingState = true;
            if (gameStateEntity != null)
            {
                gameStateEntity.Dispose();
                gameStateEntity = null;
            }

            previousGameState = currentGameState;
            currentGameState = gameState;

            PlaceholderFactory<IGameState> localFactory = FindFactoryConnectedToState(gameState);
           
            if(localFactory!=null)
            {
                IGameState currentIGameState = localFactory.Create();
                gameStateEntity = (GameStateEntity)diContainer.Resolve(currentIGameState.GetType());
                currentIGameState.Start();
                IsChangingState = false;
                //Debug.Log("State was changed to: " + gameState);
            }
            else
            {
                Debug.LogError("Factory of state "+gameState+" was not found!");
                return;
            }
        }

        private StateFactory FindFactoryConnectedToState(GameState gameState)
        {
            foreach(KeyValuePair<IGameState, StateFactory> kvp in allStates)
            {
                if(kvp.Key.ConnectedState == gameState)
                {
                    return kvp.Value;
                }
            }    
            return null;
        }
    }
}
