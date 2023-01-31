using GLShared.General.Components;
using GLShared.General.Signals;
using GLShared.Networking.Components;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Frontend.Scripts
{
    public class FrontShootingSystem : ShootingSystemBase
    {
        [SerializeField] private float reloadTime = 5f;

        public override void Initialize()
        {
            base.Initialize();
        }

        protected override void Update()
        {
            if (inputProvider.LockPlayerInput)
            {
                return;
            }

            base.Update();
            if (ShootkingKeyPressed && !isReloading)
            {
                SingleShotLogic();
            }
        }

        protected void SingleShotLogic()
        {
            isReloading = true;

            AfterShotCallback(reloadTime);
        }
    }
}
