using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
using System.Linq;

using UInc.Core.Utilities;
using System.Reflection;

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

        private readonly Dictionary<IGameState, StateFactory> allStates = new Dictionary<IGameState, StateFactory>();
        
        private IGameState currentIGameState;

    
        public void Initialize()
        {
            ChangeState(GameState.Lobby);
        }

        public GameStateManager(StateFactory[] localStateFactory,IGameState[] localAllStates)
        {
            for(int i=0; i< localAllStates.Length;i++)
            {
                allStates.Add(localAllStates[i], localStateFactory[i]);
            }
        }

        public void ChangeState(GameState gameState)
        {
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
                currentIGameState = localFactory.Create();
                currentIGameState.Startt();
                //Debug.Log("State was changed to: " + gameState);
            }
            else
            {
                Debug.LogError("Factory of state "+gameState+" was not found!");
                return;
            }
            
            // _strategy = _strategyFactory.Create();
            // _strategy.Startt();
            //gameStateEntity = gameStateFactory.CreateState(gameState);
            //  gameStateEntity.Start();
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

        private void Start() 
        {
            ChangeState(GameState.Calibration);
        }
        
       
    }
}
