using System.Linq;
using UnityEngine;
using GLShared.General.Components;

namespace Frontend.Scripts.Components
{
    public class UTSuspensionController : UTVehicleController
    {
        [Header("Additional")]
        [SerializeField] private bool airControl = true;

        protected override void FixedUpdate()
        {
            if (!runPhysics)
            {
                return;
            }

            base.FixedUpdate();

            CustomGravityLogic();

            if (!isUpsideDown)
            {
                EvaluateDriveParams();
                Accelerate();
                Brakes();
                AirControl();

                SetCurrentSpeed();
            }
        }

        protected override void CustomGravityLogic()
        {
            if (!allGroundedWheels.Where(wheel => !wheel.IsIdler).Any())
            {
                rig.AddForce(Physics.gravity, ForceMode.Acceleration);
            }
            else
            {
                if (currentFrictionPair.IsDefaultLayer)
                {
                    if (currentFrictionPair.HorizontalAnglesRange.Max >= absHorizontalAngle && CUSTOM_GRAVITY_MAX_VERTICAL_ANGLE >= absVerticalAngle)
                    {
                        //If we dont overreach maximum angle for default ground, we apply normalized gravity
                        rig.AddForce(-transform.up * Physics.gravity.magnitude, ForceMode.Acceleration);
                    }
                    else
                    {
                        //If we overreach maximum angle for default ground, we apply multiplied gravity

                        (float currentOverreachAngle, float maxAllowedAngleInDirection) = absHorizontalAngle > absVerticalAngle ?
                            (absHorizontalAngle, currentFrictionPair.HorizontalAnglesRange.Max) : (absVerticalAngle, CUSTOM_GRAVITY_MAX_VERTICAL_ANGLE);
                        float ratio = (currentOverreachAngle / maxAllowedAngleInDirection);

                        rig.AddForce(Physics.gravity * ratio, ForceMode.Acceleration);
                    }
                }
                else
                {
                    //If the ground is not a default ground then we apply multiplied gravity
                    (float currentOverreachAngle, float maxAllowedAngleInDirection) = absHorizontalAngle > absVerticalAngle ?
                            (absHorizontalAngle, currentFrictionPair.HorizontalAnglesRange.Max) : (absVerticalAngle, CUSTOM_GRAVITY_MAX_VERTICAL_ANGLE);
                    float ratio = (currentOverreachAngle / maxAllowedAngleInDirection);

                    rig.AddForce(Physics.gravity * ratio, ForceMode.Acceleration);
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
