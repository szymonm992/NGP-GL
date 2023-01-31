using Frontend.Scripts.Components;
using Frontend.Scripts.ScriptableObjects;
using Frontend.Scripts.Signals;
using GLShared.General.ScriptableObjects;
using GLShared.General.Signals;
using GLShared.Networking.Components;
using UnityEngine;
using Zenject;

namespace Frontend.Scripts
{
    public class ProjectInstaller : MonoInstaller
    {
        [SerializeField] private GameParameters gameParameters;
        [SerializeField] private FrontVisualSettings visualSettings;
        [SerializeField] private FrontSettings frontSettings;

        public override void InstallBindings()
        {
            InstallSignals();
            InstallMain(); 
        }

        private void InstallMain()
        {
            Container.Bind<SmartFoxConnection>().FromComponentInHierarchy().AsCached().NonLazy();
            Container.BindInterfacesAndSelfTo<SceneHandler>().FromComponentInHierarchy().AsSingle();

            Container.BindInterfacesAndSelfTo<ConnectionManager>().FromComponentInHierarchy().AsCached();

            Container.Bind<FormValidator>().AsSingle();
            Container.Bind<GameParameters>().FromInstance(gameParameters).AsSingle();
            Container.Bind<FrontVisualSettings>().FromInstance(visualSettings).AsSingle();
            Container.Bind<FrontSettings>().FromInstance(frontSettings).AsSingle();
        }

        private void InstallSignals()
        {
            SignalBusInstaller.Install(Container);

            Container.DeclareSignal<BattleSignals.CameraSignals.OnCameraBound>();
            Container.DeclareSignal<BattleSignals.CameraSignals.OnCameraModeChanged>();
            Container.DeclareSignal<BattleSignals.CameraSignals.OnZoomChanged>();

            Container.DeclareSignal<BattleSignals.OnGameStageUpdate>();
            Container.DeclareSignal<BattleSignals.OnCounterUpdate>().OptionalSubscriber();

            Container.DeclareSignal<PlayerSignals.OnPlayerInitialized>();
            Container.DeclareSignal<PlayerSignals.OnPlayerSpawned>();
            Container.DeclareSignal<PlayerSignals.OnAllPlayersInputLockUpdate>();
            Container.DeclareSignal<PlayerSignals.OnAllPlayersInputLockUpdate>();
            Container.DeclareSignal<PlayerSignals.OnBattleTimeChanged>();

            Container.DeclareSignal<ShellSignals.OnShellSpawned>();

            Container.DeclareSignal<ConnectionSignals.OnConnectionAttemptResult>();
            Container.DeclareSignal<ConnectionSignals.OnLoginAttemptResult>();
            Container.DeclareSignal<ConnectionSignals.OnDisconnectedFromServer>();
            Container.DeclareSignal<ConnectionSignals.OnLobbyJoinAttemptResult>();
            Container.DeclareSignal<ConnectionSignals.OnServerSettingsResponse>();
            Container.DeclareSignal<ConnectionSignals.OnRoomJoinResponse>();
            Container.DeclareSignal<ConnectionSignals.OnPingUpdate>();
            Container.DeclareSignal<ConnectionSignals.OnCancelEnteringBattle>();
            Container.DeclareSignal<ConnectionSignals.OnBattleJoiningInfoReceived>();
        }

        public void OnValidate()
        {
            #if UNITY_EDITOR
            Debug.unityLogger.logEnabled = true;
            #endif
        }
    }
}
