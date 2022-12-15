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
using GLShared.Networking.Components;
using UnityEngine.TextCore.Text;

namespace Frontend.Scripts
{
    public class GameSceneInstaller : MonoInstaller
    {
        [SerializeField] private GameParameters gameParameters;
        [SerializeField] private RandomBattleParameters randomBattleParameters;

        public override void InstallBindings()
        {
            InstallSignals();
            InstallMain();
            InstallTemporary();
        }

        private void InstallSignals()
        {
            SignalBusInstaller.Install(Container);

            Container.DeclareSignal<BattleSignals.CameraSignals.OnCameraBound>();
            Container.DeclareSignal<BattleSignals.CameraSignals.OnCameraModeChanged>();
            Container.DeclareSignal<PlayerSignals.OnPlayerInitialized>();
            Container.DeclareSignal<PlayerSignals.OnPlayerSpawned>();
            Container.DeclareSignal<PlayerSignals.OnAllPlayersInputLockUpdate>();

            Container.DeclareSignal<ConnectionSignals.OnConnectionAttemptResult>();
            Container.DeclareSignal<ConnectionSignals.OnLoginAttemptResult>();
            Container.DeclareSignal<ConnectionSignals.OnDisconnectedFromServer>();
            Container.DeclareSignal<ConnectionSignals.OnLobbyJoinAttemptResult>();
        }

        private void InstallMain()
        {

            Container.Bind<SmartFoxConnection>().FromComponentInHierarchy().AsCached();
            Container.BindInterfacesAndSelfTo<ConnectionManager>().FromComponentInHierarchy().AsCached();

            //spawning players==
            Container.BindInterfacesAndSelfTo<PlayerSpawner>().FromNewComponentOnNewGameObject().AsCached().NonLazy();
            Container.Bind<PlayerProperties>().FromInstance(new PlayerProperties()).AsCached();
            Container.BindFactory<PlayerEntity, PlayerProperties, PlayerEntity, PlayerSpawner.Factory>().FromSubContainerResolve().ByInstaller<PlayerSpawner.PlayerInstaller>();
            //==================

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
