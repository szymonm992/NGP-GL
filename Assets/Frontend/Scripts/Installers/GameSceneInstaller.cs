using UnityEngine;
using Zenject;
using Frontend.Scripts.Models;
using Frontend.Scripts.Components;
using Frontend.Scripts.Signals;
using Frontend.Scripts.Interfaces;
using GLShared.Networking;
using GLShared.General.ScriptableObjects;

namespace Frontend.Scripts
{
    public class GameSceneInstaller : MonoInstaller
    {
        [SerializeField] private GameParameters gameParameters;

        public override void InstallBindings()
        {
            InstallMain();
            InstallSignals();
        }

      
        private void InstallSignals()
        {
            SignalBusInstaller.Install(Container);

            Container.DeclareSignal<BattleSignals.CameraSignals.OnCameraBound>();
            Container.DeclareSignal<BattleSignals.CameraSignals.OnCameraZoomChanged>();
        }

        private void InstallMain()
        {
            Container.Bind<SmartFoxConnection>().FromComponentInHierarchy().AsCached();
            Container.Bind<ConnectionManager>().FromComponentInHierarchy().AsCached();
            Container.BindInterfacesAndSelfTo<UIManager>().FromComponentInHierarchy().AsSingle();
            Container.Bind<FormValidator>().AsSingle();
            Container.Bind<GameParameters>().FromInstance(gameParameters).AsSingle();

        }

        public void OnValidate()
        {
            #if UNITY_EDITOR
            Debug.unityLogger.logEnabled = true;
            #endif
        }
    }

 
}
