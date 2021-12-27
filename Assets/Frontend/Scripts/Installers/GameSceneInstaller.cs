using UnityEngine;
using Zenject;

using Frontend.Scripts.Components;

namespace Frontend.Scripts
{
    public class GameSceneInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            InstallGameStates();
            InstallMain();
        }
        private void InstallGameStates()
        {
            Container.Bind<GameStateFactory>().AsSingle();
            Container.BindInterfacesAndSelfTo<CalibrationState>().AsSingle();
            Container.BindFactory<CalibrationState, CalibrationState.Factory>().WhenInjectedInto<GameStateFactory>();
        }
        private void InstallMain()
        {
            Container.Bind<SmartFoxConnection>().FromComponentInHierarchy().AsCached();
        }
    }
}
