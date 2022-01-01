using UnityEngine;
using Zenject;

using System;

using Frontend.Scripts.Models;
using Frontend.Scripts.Components;
using Frontend.Scripts.Signals;

namespace Frontend.Scripts
{
    public class GameSceneInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            InstallGameStates();
            InstallMain();
            InstallSignals();
        }

        private void InstallGameStates()
        {
            Container.Bind<AsyncProcessor>().FromComponentInHierarchy().AsCached().NonLazy();
            Container.BindInterfacesAndSelfTo<GameStateManager>().FromComponentInHierarchy().AsCached().NonLazy();
            
            var fields = typeof(GameState).GetEnumValues();
            foreach(var fi in fields)
            {
                var state = (GameState)fi;
                var field = typeof(GameState).GetField(state.ToString());
                if (field.IsDefined(typeof(GameStateEntityAttribute), false))
                {
                    var baseClassType = state.GetTypeOfBaseClass();
                    Container.BindInterfacesAndSelfTo(baseClassType).AsSingle();
                    //Container.BindInterfacesAndSelfTo2(baseClassType).AsSingle().NonLazy();
                    Container.BindFactory<IGameState, StateFactory>().To2(baseClassType);
                    Container.BindInstance<GameState>(state).WhenInjectedInto(baseClassType);
                }
            }
        }

        private void InstallSignals()
        {
            SignalBusInstaller.Install(Container);
            Container.DeclareSignal<GameStateChangedSignal>();
        }

        private void InstallMain()
        {
            Container.Bind<SmartFoxConnection>().FromComponentInHierarchy().AsCached();
            Container.Bind<ConnectionManager>().FromComponentInHierarchy().AsCached();
            Container.BindInterfacesAndSelfTo<UIManager>().FromComponentInHierarchy().AsSingle();
            Container.Bind<FormValidator>().AsSingle();
            
            
        }
    }

 
}
