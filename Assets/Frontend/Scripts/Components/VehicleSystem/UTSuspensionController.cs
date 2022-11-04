using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;
using Frontend.Scripts.Interfaces;
using Frontend.Scripts.Models;
using GLShared.General.Enums;
using GLShared.General.ScriptableObjects;

namespace Frontend.Scripts.Components
{
    public class UTSuspensionController : UTVehicleController, IVehicleController
    {
        [Header("Additional")]
        [SerializeField] private bool airControl = true;


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
                AirControl();

                SetCurrentSpeed();
                speedometer?.SetSpeedometr(currentSpeed);
            }
        }

        private void CustomGravityLogic()
        {

            int groundedWheelsAmount = allGroundedWheels.Count();

            if (!allGroundedWheels.Any())
            {
                rig.AddForce(Physics.gravity, ForceMode.Acceleration);
            }
            else if (allWheelsAmount == groundedWheelsAmount)
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
            else
            {
                var notGroundedAmount = (allWheelsAmount - groundedWheelsAmount);
                foreach (var wheel in allWheels)
                {
                    if (!wheel.IsGrounded)
                    {
                        rig.AddForceAtPosition((Physics.gravity / notGroundedAmount), wheel.transform.position, ForceMode.Acceleration);
                    }
                }
            }
        }

        private void AirControl()
        {
            if (!airControl)
            {
                return;
            }

            if (!allGroundedWheels.Any())
            {
                float angle = transform.eulerAngles.x;
                angle = (angle > 180f) ? angle - 360f : angle;

                if (Mathf.Abs(angle) > gameParameters.AirControlAngleThreshold)
                {
                    rig.AddTorque(transform.right * (gameParameters.AirControlForce * -Mathf.Sign(angle)), ForceMode.Acceleration);
                }
            }
        }
    }
}
