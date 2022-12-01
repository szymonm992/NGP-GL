using UnityEngine;
using Zenject;
using Frontend.Scripts.Models;
using Frontend.Scripts.Components;
using Frontend.Scripts.Signals;
using Frontend.Scripts.Interfaces;
using GLShared.Networking;
using GLShared.General.ScriptableObjects;
using Frontend.Scripts.Components.Temporary;
using GLShared.General.Signals;

namespace Frontend.Scripts
{
    public class GameSceneInstaller : MonoInstaller
    {
        [SerializeField] private GameParameters gameParameters;

        public override void InstallBindings()
        {
            InstallMain();
            InstallSignals();
            InstallTemporary();
        }

      
        private void InstallSignals()
        {
            SignalBusInstaller.Install(Container);

            Container.DeclareSignal<BattleSignals.CameraSignals.OnCameraBound>();
            Container.DeclareSignal<BattleSignals.CameraSignals.OnCameraModeChanged>();
            Container.DeclareSignal<PlayerSignals.OnLocalPlayerInitialized>();
        }

        private void InstallMain()
        {
            Container.Bind<SmartFoxConnection>().FromComponentInHierarchy().AsCached();
            Container.Bind<ConnectionManager>().FromComponentInHierarchy().AsCached();
            Container.BindInterfacesAndSelfTo<ReticleController>().FromComponentInHierarchy().AsSingle();


          
            Container.Bind<FormValidator>().AsSingle();

            Container.Bind<GameParameters>().FromInstance(gameParameters).AsSingle();
        }

        private void InstallTemporary() //TO BE REMOVED WHEN ACTUAL GAME ARCHITECTURE COMES IN
        {
            Container.BindInterfacesAndSelfTo<TempGameManager>().FromComponentInHierarchy().AsCached();
        }

        public void OnValidate()
        {
            #if UNITY_EDITOR
            Debug.unityLogger.logEnabled = true;
            #endif
        }
    }

 
}
