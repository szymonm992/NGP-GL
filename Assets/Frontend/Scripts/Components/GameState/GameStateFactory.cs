using ModestTree;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Zenject;

namespace Frontend.Scripts.Components
{
    //all the available states
  


    //the constructor for the state factory
    public class GameStateFactory
    {
      // private readonly CalibrationState.Factory calibrationFactory;
        private DiContainer container;

        StateFactory[] stateFactories;
        private IGameState currentIGameState;

        public GameStateFactory(StateFactory[] localFactories)//,
                                //CalibrationState.Factory localCalibrationFactory)
        {
            stateFactories = localFactories;
            Debug.Log(stateFactories.Length);
            // calibrationFactory = localCalibrationFactory;
        }

        // Creates the requested game state entity
        public GameStateEntity CreateState(GameState gameState)
        {
            var field = typeof(GameState).GetField(gameState.ToString());
            var hasFactoryAndBaseClass = field.IsDefined(typeof(PlaceholderFactoryAttribute), false) && field.IsDefined(typeof(GameStateEntityAttribute),false);
            if (hasFactoryAndBaseClass)
            {
                var typeOfAttribute = gameState.GetTypeOfState();
                var baseClassType = gameState.GetTypeOfBaseClass();
               // var obb = container.Resolve(typeOfAttribute);
                //container.TryResolve(typeOfAttribute);
            }

            IGameState state =  stateFactories[0].Create();
            Debug.Log(state.GetType().BaseType);
            switch (gameState)
            {
                //case GameState.Calibration:
                  //  return calibrationFactory.Create();
                default:
                {
                        return null;
                    //when no state matches we throw an exception(just in case)
                    throw Assert.CreateException("Exception found... just saying...");
                }  
            }
        }
    }
    

    }
