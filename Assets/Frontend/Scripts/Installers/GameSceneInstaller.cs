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
using GLShared.General.Models;

namespace Frontend.Scripts
{
    public class GameSceneInstaller : MonoInstaller
    {
        [SerializeField] private GameParameters gameParameters;
        [SerializeField] private RandomBattleParameters randomBattleParameters;
        [SerializeField] private PlayerEntity testGoprefab;
        
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
            Container.DeclareSignal<PlayerSignals.OnPlayerInitialized>();
            Container.DeclareSignal<PlayerSignals.OnPlayerSpawned>();
        }

        private void InstallMain()
        {
            Container.Bind<SmartFoxConnection>().FromComponentInHierarchy().AsCached();
            Container.Bind<ConnectionManager>().FromComponentInHierarchy().AsCached();

            Container.BindFactory<PlayerProperties, PlayerEntity, PlayerEntity.Factory>().FromComponentInNewPrefab(testGoprefab);

            Container.BindInterfacesAndSelfTo<ReticleController>().FromComponentInHierarchy().AsSingle();

            Container.Bind<FormValidator>().AsSingle();
            Container.Bind<GameParameters>().FromInstance(gameParameters).AsSingle();
            Container.BindInterfacesAndSelfTo<RandomBattleParameters>().FromInstance(randomBattleParameters).AsSingle();
        }

        private void InstallTemporary() 
        {
            
        }

        public void OnValidate()
        {
            #if UNITY_EDITOR
            Debug.unityLogger.logEnabled = true;
            #endif
        }
    }

 
}
