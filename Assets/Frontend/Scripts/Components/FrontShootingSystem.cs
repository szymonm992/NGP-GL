using GLShared.General.Components;
using GLShared.General.Signals;
using UnityEngine;
using Zenject;

namespace Frontend.Scripts.Components
{
    public class FrontShootingSystem : ShootingSystemBase
    {
        [Inject] private readonly SignalBus signalBus;
        [Inject] private readonly ReloadMeter reloadMeter;
        
        [SerializeField] private float reloadTime = 5f;

        public override void Initialize()
        {
            base.Initialize();

            signalBus.Subscribe<ShellSignals.OnShellSpawned>(OnShellSpawned);
        }

        protected override void Update()
        {
            if (inputProvider.LockPlayerInput)
            {
                return;
            }

            base.Update();

            reloadMeter.DisplayCurrentReload(currentReloadTimer);
        }


        private void OnShellSpawned(ShellSignals.OnShellSpawned OnShellSpawned)
        {
            isReloading = true;

            AfterShotCallback(reloadTime);
        }
    }
}
