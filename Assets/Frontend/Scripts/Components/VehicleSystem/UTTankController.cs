using Frontend.Scripts.Interfaces;
using Frontend.Scripts.Models;
using GLShared.General.Enums;
using GLShared.General.ScriptableObjects;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
using System.Linq;

namespace Frontend.Scripts.Components
{
    public class UTTankController : UTVehicleController
    {

        protected override void FixedUpdate()
        {
            base.FixedUpdate();

            CustomGravityLogic();

            if (!isUpsideDown)
            {
                EvaluateDriveParams();
                Accelerate();
                Brakes();
                ApplyFrictionForces();

                SetCurrentSpeed();
                speedometer?.SetSpeedometr(currentSpeed);
            }
        }


        private void CustomGravityLogic()
        {
            if (!allGroundedWheels.Any())
            {
                rig.AddForce(Physics.gravity, ForceMode.Acceleration);
            }
            else
            {
                float angle = Vector3.Angle(transform.up, -Physics.gravity.normalized);

                if (maxSlopeAngle >= angle)
                {
                    rig.AddForce(-transform.up * Physics.gravity.magnitude, ForceMode.Acceleration);
                }
                else
                {
                    rig.AddForce(Physics.gravity, ForceMode.Acceleration);
                }
            }
        }
    }
}
