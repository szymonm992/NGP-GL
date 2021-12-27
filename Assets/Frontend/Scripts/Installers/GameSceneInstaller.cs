using UnityEngine;
using Zenject;

using Frontend.Scripts.Components;
using System.Reflection;
using System;

namespace Frontend.Scripts
{
    public class GameSceneInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            InstallGameStates();
            InstallMain();
        }
        private void InstallGameStates()
        {
            Container.Bind<GameStateFactory>().AsSingle();

            var fields = typeof(GameState).GetEnumValues();
            foreach(var fi in fields)
            {
                var state = (GameState)fi;
                var field = typeof(GameState).GetField(state.ToString());
                var hasFactoryAndBaseClass = field.IsDefined(typeof(PlaceholderFactoryAttribute), false) && field.IsDefined(typeof(GameStateEntityAttribute),false);
                if (hasFactoryAndBaseClass)
                {
                    var typeOfAttribute = state.GetTypeOfState();
                    var baseClassType = state.GetTypeOfBaseClass();


                    Debug.Log("Base type: " + baseClassType);
                    Container.BindInterfacesAndSelfTo2(baseClassType).AsSingle();
                    Debug.Log("State: " + state.ToString() + " has type of " + typeOfAttribute);
                   // Container.BindFactory2<IGameState>(typeOfAttribute);

                }
        
            }
            //Container.BindInterfacesAndSelfTo<CalibrationState>().AsSingle();
            //Container.BindFactory<CalibrationState, CalibrationState.Factory>().WhenInjectedInto<GameStateFactory>();
        }
        private void InstallMain()
        {
            Container.Bind<SmartFoxConnection>().FromComponentInHierarchy().AsCached();
        }
    }

 
}
