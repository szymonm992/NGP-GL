using Frontend.Scripts.Components;
using GLShared.General.Components;
using GLShared.General.Interfaces;
using GLShared.General.Models;
using GLShared.General.ScriptableObjects;
using GLShared.Networking.Components;
using Prototyping.Scripts.Components;
using UnityEngine;
using Zenject;

namespace Prototyping.Scripts.Installers
{
    public class PrototypingSceneInstaller : MonoInstaller
    {
        [SerializeField] private RandomBattleParameters randomBattleParameters;
        [SerializeField] private VehiclesDatabase vehiclesDatabase;

        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<PrototypingLevelManager>().FromComponentInHierarchy().AsSingle();
            Container.BindInterfacesAndSelfTo<ReticleController>().FromComponentInHierarchy().AsSingle();

            Container.BindInterfacesAndSelfTo<PlayerSpawner>().FromNewComponentOnNewGameObject().AsCached().NonLazy();
            Container.Bind<PlayerProperties>().FromInstance(new PlayerProperties()).AsCached();
            Container.BindFactory<PlayerEntity, PlayerProperties, PlayerEntity, PlayerSpawner.Factory>().FromSubContainerResolve().ByInstaller<PlayerSpawner.PlayerInstaller>();

            Container.BindInterfacesAndSelfTo<RandomBattleParameters>().FromInstance(randomBattleParameters).AsSingle();
            Container.Bind<Speedometer>().FromComponentInHierarchy().AsSingle();

            Container.BindInterfacesAndSelfTo<IVehiclesDatabase>().FromInstance(vehiclesDatabase).AsCached();
            Container.BindInterfacesAndSelfTo<ISyncManager>().FromComponentInHierarchy().AsSingle();

        }
    }
}
