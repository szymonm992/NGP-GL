using UnityEngine;
using Zenject;

using System;

using Frontend.Scripts.Models;
using Frontend.Scripts.Components;
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
            Container.BindInterfacesAndSelfTo<GameStateManager>().AsSingle();

            var fields = typeof(GameState).GetEnumValues();
            foreach(var fi in fields)
            {
                var state = (GameState)fi;
                var field = typeof(GameState).GetField(state.ToString());
                var hasBaseClassAttrib = field.IsDefined(typeof(GameStateEntityAttribute),false);
                if (hasBaseClassAttrib)
                {
                    var baseClassType = state.GetTypeOfBaseClass();
                    //Debug.Log("Base type: " + baseClassType);
                    Container.BindInterfacesAndSelfTo2(baseClassType).AsSingle();
                    //Debug.Log("State: " + state.ToString() + " has type of " + typeOfAttribute);
                    Container.BindFactory<IGameState, StateFactory>().To2(baseClassType);

                    Container.BindInstance<GameState>(state).WhenInjectedInto(baseClassType);
                    //Container.BindFactory2<IGameState>(typeOfAttribute).To2(baseClassType);
                }
            }
        }
        private void InstallMain()
        {
            Container.Bind<SmartFoxConnection>().FromComponentInHierarchy().AsCached();
        }
    }

 
}
