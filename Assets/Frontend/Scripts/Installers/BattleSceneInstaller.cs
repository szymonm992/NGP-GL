using Frontend.Scripts.Components;
using GLShared.General.Models;
using GLShared.General.ScriptableObjects;
using GLShared.Networking.Components;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
namespace Frontend.Scripts
{
    public class BattleSceneInstaller : MonoInstaller
    {
        [SerializeField] private RandomBattleParameters randomBattleParameters;

        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<TimeManager>().FromComponentInHierarchy().AsSingle();

            Container.BindInterfacesAndSelfTo<ReticleController>().FromComponentInHierarchy().AsSingle();
            Container.BindInterfacesAndSelfTo<PlayerSpawner>().FromNewComponentOnNewGameObject().AsCached().NonLazy();
            Container.Bind<PlayerProperties>().FromInstance(new PlayerProperties()).AsCached();
            Container.BindFactory<PlayerEntity, PlayerProperties, PlayerEntity, PlayerSpawner.Factory>().FromSubContainerResolve().ByInstaller<PlayerSpawner.PlayerInstaller>();

            Container.BindInterfacesAndSelfTo<RandomBattleParameters>().FromInstance(randomBattleParameters).AsSingle();
            Container.Bind<FrontendSyncManager>().FromComponentInHierarchy().AsSingle();
        }
    }
}
