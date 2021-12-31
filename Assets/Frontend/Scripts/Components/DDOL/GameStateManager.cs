using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
using System.Linq;
using Frontend.Scripts.Models;

namespace Frontend.Scripts.Components
{
    public class GameStateManager : MonoBehaviour, IInitializable
    {
        [Inject] private readonly AsyncProcessor asyncProcessor;

        [Inject] private DiContainer diContainer;
        [Inject] private StateFactory[] localStateFactory;
        [Inject] private IGameState[] localAllStates;

        private Dictionary<IGameState, StateFactory> allStates;

        private GameState currentGameState;
        private GameState previousGameState;

        private GameStateEntity gameStateEntity = null;

        public GameState CurrentGameState => currentGameState;
        public GameState PeeviousGameState => previousGameState;

        public bool IsChangingState { get; set; }


         

       /*
        public GameStateManager(DiContainer localDiContainer, StateFactory[] localStateFactory,IGameState[] localAllStates)
        {
            diContainer = localDiContainer;
            for (int i=0; i< localAllStates.Length;i++)
            {
                allStates.Add(localAllStates[i], localStateFactory[i]);
            }
            Debug.Log(allStates.Count);
        }*/

        public void ChangeStateDelayed(GameState gameState, float delayInSeconds)
        {
            if (IsChangingState)
            {
                Debug.LogError("Cannot change state while the state is being changed already");
                return;
            }
            IsChangingState = true;
            asyncProcessor.StartNewCoroutine(()=> ChangeState(gameState), delayInSeconds);
        }

        public void ChangeState(GameState gameState)
        {
            if(IsChangingState)
            {
                Debug.LogError("Cannot change state while the state is being changed already");
                return;
            }

            if (currentGameState == gameState) return;

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

        public void Initialize()
        {
            Debug.Log(diContainer == null);
            localAllStates = diContainer.Resolve<IGameState[]>();
            localStateFactory = diContainer.Resolve<StateFactory[]>();
            Debug.Log(localAllStates.Length);
            Debug.Log(localStateFactory.Length);
            allStates = new Dictionary<IGameState, StateFactory>();
            for (int i = 0; i < localAllStates.Length; i++)
            {
                allStates.Add(localAllStates[i], localStateFactory[i]);
            }
        }
    }
}
