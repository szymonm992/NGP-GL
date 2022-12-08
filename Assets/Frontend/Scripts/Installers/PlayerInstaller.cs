using Frontend.Scripts.Interfaces;
using Frontend.Scripts.Models;
using GLShared.General.Components;
using GLShared.General.Interfaces;
using GLShared.General.Models;
using GLShared.Networking.Components;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Frontend.Scripts.Components
{
    public class PlayerInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.BindInitializableExecutionOrder<PlayerEntity>(+10);

            Container.BindInterfacesAndSelfTo<UTAxleBase>().FromComponentsInHierarchy().AsCached();
            Container.BindInterfacesAndSelfTo<UTPhysicWheelBase>().FromComponentsInHierarchy().AsCached().NonLazy();

            Container.Bind<Renderer>().FromComponentsInChildren().AsCached();
            Container.BindInterfacesAndSelfTo<WheelDummy>().FromComponentsInHierarchy().AsCached();
            Container.BindInterfacesAndSelfTo<VehicleModelEffects>().FromNewComponentOnRoot().AsCached();
        }
    }
}
