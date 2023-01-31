using Frontend.Scripts.Components;
using GLShared.General.Components;
using GLShared.General.Models;
using GLShared.General.ScriptableObjects;
using GLShared.Networking.Components;
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

            //PLAYER SPAWNING LOGIC
            Container.BindInterfacesAndSelfTo<PlayerSpawner>().FromNewComponentOnNewGameObject().AsCached().NonLazy();
            Container.Bind<PlayerProperties>().FromInstance(new PlayerProperties()).AsCached();
            Container.BindFactory<PlayerEntity, PlayerProperties, PlayerEntity, PlayerSpawner.Factory>().FromSubContainerResolve().ByInstaller<PlayerSpawner.PlayerInstaller>();

            //SHELL SPAWNING LOGIC
            Container.BindInterfacesAndSelfTo<ShellSpawner>().FromNewComponentOnNewGameObject().AsCached().NonLazy();
            Container.Bind<ShellProperties>().FromInstance(new ShellProperties()).AsCached();
            Container.BindFactory<ShellEntity, ShellProperties, ShellEntity, ShellSpawner.Factory>().FromSubContainerResolve()
                .ByInstaller<ShellSpawner.ShellInstaller>();

            Container.BindInterfacesAndSelfTo<RandomBattleParameters>().FromInstance(randomBattleParameters).AsSingle();
            Container.Bind<FrontendSyncManager>().FromComponentInHierarchy().AsSingle();

            Container.Bind<Speedometer>().FromComponentInHierarchy().AsSingle();
        }
    }
}
