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
        [SerializeField] private bool airControl = true;

        private void Update()
        {
            if (inputProvider != null)
            {
                isBrake = inputProvider.Brake;
                inputY = inputProvider.Vertical;

                absoluteInputY = inputProvider.AbsoluteVertical;

                signedInputY = inputProvider.SignedVertical;
            }
        }

        private void FixedUpdate()
        {
            allGroundedWheels = GetGroundedWheelsInAllAxles();

            CustomGravityLogic();
            EvaluateDriveParams();
            Accelerate();
            Brakes();
            ApplyFrictionForces();
            AirControl();

            currentSpeed = rig.velocity.magnitude * gameParameters.SpeedMultiplier;
            speedometer?.SetSpeedometr(currentSpeed);
        }

        private void EvaluateDriveParams()
        {
            currentDriveForce = enginePowerCurve.Evaluate(currentSpeed);
        }

        private void ApplyFrictionForces()
        {
            if(!allGroundedWheels.Any())
            {
                return;
            }

            foreach (var wheel in allGroundedWheels)
            {
                Vector3 steeringDir = wheel.transform.right;
                Vector3 tireVel = rig.GetPointVelocity(wheel.HitInfo.Point);

                float steeringVel = Vector3.Dot(steeringDir, tireVel);
                float desiredVelChange = -steeringVel * wheel.SidewaysTireGripFactor;
                float desiredAccel = desiredVelChange / Time.fixedDeltaTime;

                rig.AddForceAtPosition(desiredAccel * wheel.TireMass * steeringDir, wheel.HitInfo.Point);
            }
        }

        private void Accelerate()
        {
            if (absoluteInputY == 0 || isBrake)
            {
                return;
            }

            foreach (var axle in allAxles)
            {
                if (axle.CanDrive && !isBrake)
                {
                    var groundedWheels = axle.GetGroundedWheels();

                    if (!groundedWheels.Any())
                    {
                        return;
                    }

                    foreach (var wheel in groundedWheels)
                    {
                        if (wheel.HitInfo.NormalAndUpAngle <= gameParameters.MaxWheelDetectionAngle)
                        {
                            wheelVelocityLocal = wheel.transform.InverseTransformDirection(rig.GetPointVelocity(wheel.UpperConstraintPoint));

                            forwardForce = inputY * currentDriveForce;
                            turnForce = wheelVelocityLocal.x * currentDriveForce;

                            rig.AddForceAtPosition((forwardForce * wheel.transform.forward), wheel.HitInfo.Point);
                            rig.AddForceAtPosition((turnForce * -wheel.transform.right), wheel.UpperConstraintPoint);
                        }
                    }
                }
            }
        }


        private void Brakes()
        {
            if (!allGroundedWheels.Any())
            {
                return;
            }

            currentLongitudalGrip = isBrake ? 1f : (absoluteInputY > 0 ? 0 : 0.5f);

            if (absoluteInputY == 0 || isBrake)
            {
                foreach (var wheel in allGroundedWheels)
                {
                    Vector3 forwardDir = wheel.transform.forward;
                    Vector3 tireVel = rig.GetPointVelocity(wheel.UpperConstraintPoint);

                    float steeringVel = Vector3.Dot(forwardDir, tireVel);
                    float desiredVelChange = -steeringVel * currentLongitudalGrip;
                    float desiredAccel = desiredVelChange / Time.fixedDeltaTime;

                    rig.AddForceAtPosition(desiredAccel * wheel.TireMass * forwardDir, wheel.UpperConstraintPoint);
                }
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
