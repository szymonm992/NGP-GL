using Frontend.Scripts.Models;
using GLShared.General.Components;
using GLShared.General.Enums;
using GLShared.General.Interfaces;
using GLShared.General.Models;
using GLShared.General.ScriptableObjects;
using GLShared.General.Signals;
using GLShared.Networking.Components;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Zenject;

namespace Frontend.Scripts.Components
{
    public class UTFrontController : UTVehicleController
    {
        [Inject] private readonly FrontVisualSettings visualSettings;
        [Inject(Optional = true)] private readonly Outline outline;

        public override bool RunPhysics => false;

        public override void Initialize()
        {
            base.Initialize();

            if (outline != null)
            {
                outline.OutlineWidth = visualSettings.OutlineWidth;
                outline.OutlineColor = visualSettings.OutlineColor;
                outline.OutlineMode = Outline.Mode.OutlineHidden;
            }
        }

        protected override void FixedUpdate()
        {
            allGroundedWheels = GetGroundedWheelsInAllAxles().ToArray();
            isUpsideDown = CheckUpsideDown();

            if (!isUpsideDown)
            {
                currentSpeed = playerEntity.EntityVelocity;
            }
        }

        protected override void Update()
        {
        }
    }
}
