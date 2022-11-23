using Frontend.Scripts.Interfaces;
using Frontend.Scripts.Models;
using GLShared.General.Components;
using GLShared.General.Interfaces;
using GLShared.General.Models;
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
            /*Container.BindInterfacesAndSelfTo<IVehicleStats>().FromComponentOnRoot().AsCached().NonLazy();
            Container.BindInterfacesAndSelfTo<PCSinglePlayerInput>().FromComponentOnRoot().AsCached().NonLazy();

            Container.BindInterfacesAndSelfTo<UTVehicleController>().FromComponentOnRoot().AsCached().NonLazy();
            Container.BindInterfacesAndSelfTo<UTTankSteering>().FromComponentOnRoot().AsCached().NonLazy();
            Container.BindInterfacesAndSelfTo<WheelRepositionBase>().FromComponentOnRoot().AsCached().NonLazy();


            Container.BindInterfacesAndSelfTo<UTWheel>().FromComponentsInChildren().AsCached();
            Container.BindInterfacesAndSelfTo<UTAxle>().FromComponentsInChildren().AsCached();*/

            Container.Bind<Renderer>().FromComponentsInChildren().AsCached();
            Container.BindInterfacesAndSelfTo<WheelDummy>().FromComponentsInHierarchy().AsCached();
            Container.BindInterfacesAndSelfTo<VehicleModelEffects>().FromNewComponentOnRoot().AsCached();
        }
    }
}
