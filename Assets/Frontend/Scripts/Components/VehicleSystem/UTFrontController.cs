using Frontend.Scripts.Models;
using GLShared.General.Enums;
using GLShared.General.Interfaces;
using GLShared.General.Models;
using GLShared.General.ScriptableObjects;
using GLShared.General.Signals;
using GLShared.Networking.Components;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

namespace Frontend.Scripts.Components
{
    public class UTFrontController : UTVehicleController
    {
        public override bool RunPhysics => false;

        protected override void FixedUpdate()
        {
            allGroundedWheels = GetGroundedWheelsInAllAxles().ToArray();
            isUpsideDown = CheckUpsideDown();
            if (!isUpsideDown)
            {
                currentSpeed = playerEntity.EntityVelocity;
                Debug.Log("speed "+ currentSpeed);
            }
        }

        protected override void Update()
        {

        }
    }
}
