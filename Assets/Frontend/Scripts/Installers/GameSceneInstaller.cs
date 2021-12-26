using UnityEngine;
using Zenject;

using Frontend.Scripts.Components;

namespace Frontend.Scripts
{
    public class GameSceneInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            InstallMain();
        }

        private void InstallMain()
        {
            Container.Bind<SmartFoxConnection>().FromComponentInHierarchy().AsCached();
        }
    }
}
