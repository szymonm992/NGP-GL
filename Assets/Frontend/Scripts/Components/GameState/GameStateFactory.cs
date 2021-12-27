using ModestTree;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Frontend.Scripts.Components
{
    //all the available states
  


    //the constructor for the state factory
    public class GameStateFactory
    {
       private readonly CalibrationState.Factory calibrationFactory;

        public GameStateFactory(
                                CalibrationState.Factory localCalibrationFactory)
        {
            calibrationFactory = localCalibrationFactory;
        }

        // Creates the requested game state entity
        internal GameStateEntity CreateState(GameState gameState)
        {
            
            switch (gameState)
            {
                case GameState.Calibration:
                    return calibrationFactory.Create();
                default:
                {
                    //when no state matches we throw an exception(just in case)
                    throw Assert.CreateException("Exception found... just saying...");
                }  
            }
        }
    }

}
