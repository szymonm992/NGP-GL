using Frontend.Scripts.Components;
using Frontend.Scripts.Signals;
using GLShared.General.Models;
using GLShared.General.ScriptableObjects;
using GLShared.General.Signals;
using GLShared.Networking.Components;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Frontend.Scripts
{
    public class ProjectInstaller : MonoInstaller
    {
        [SerializeField] private GameParameters gameParameters;
        [SerializeField] private FrontVisualSettings visualSettings;

        public override void InstallBindings()
        {
            InstallSignals();
            InstallMain(); 
        }



        private void InstallMain()
        {
            //
            //==================

            Container.Bind<SmartFoxConnection>().FromComponentInHierarchy().AsCached().NonLazy();
            Container.BindInterfacesAndSelfTo<SceneHandler>().FromComponentInHierarchy().AsSingle();

            Container.BindInterfacesAndSelfTo<ConnectionManager>().FromComponentInHierarchy().AsCached();

            Container.Bind<FormValidator>().AsSingle();
            Container.Bind<GameParameters>().FromInstance(gameParameters).AsSingle();
            Container.Bind<FrontVisualSettings>().FromInstance(visualSettings).AsSingle();

        }

        private void InstallSignals()
        {
            SignalBusInstaller.Install(Container);

            Container.DeclareSignal<BattleSignals.CameraSignals.OnCameraBound>();
            Container.DeclareSignal<BattleSignals.CameraSignals.OnCameraModeChanged>();
            Container.DeclareSignal<BattleSignals.OnGameStageUpdate>();
            Container.DeclareSignal<BattleSignals.OnCounterUpdate>().OptionalSubscriber();

            Container.DeclareSignal<PlayerSignals.OnPlayerInitialized>();
            Container.DeclareSignal<PlayerSignals.OnPlayerSpawned>();
            Container.DeclareSignal<PlayerSignals.OnAllPlayersInputLockUpdate>();

            Container.DeclareSignal<ConnectionSignals.OnConnectionAttemptResult>();
            Container.DeclareSignal<ConnectionSignals.OnLoginAttemptResult>();
            Container.DeclareSignal<ConnectionSignals.OnDisconnectedFromServer>();
            Container.DeclareSignal<ConnectionSignals.OnLobbyJoinAttemptResult>();
            Container.DeclareSignal<ConnectionSignals.OnServerSettingsResponse>();
            Container.DeclareSignal<ConnectionSignals.OnRoomJoinResponse>();
            Container.DeclareSignal<ConnectionSignals.OnPingUpdate>();
            Container.DeclareSignal<ConnectionSignals.OnCancelEnteringBattle>();
        }

        public void OnValidate()
        {
            #if UNITY_EDITOR
            Debug.unityLogger.logEnabled = true;
            #endif
        }
    }
}
