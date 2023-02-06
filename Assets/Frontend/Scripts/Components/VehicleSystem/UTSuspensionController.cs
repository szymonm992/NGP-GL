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

            if (!allGroundedWheels.Any())
            {
                rig.AddForce(Physics.gravity, ForceMode.Acceleration); 
            }
            else
            {
                if (CUSTOM_GRAVITY_MAX_HORIZONTAL_ANGLE >= horizontalAngle && CUSTOM_GRAVITY_MAX_VERTICAL_ANGLE >= verticalAngle)
                {
                    rig.AddForce(-transform.up * Physics.gravity.magnitude, ForceMode.Acceleration);
                }
                else
                {
                    (float angle, float maxAngle) anglesPair = horizontalAngle > verticalAngle ?
                        (horizontalAngle, CUSTOM_GRAVITY_MAX_HORIZONTAL_ANGLE) : (verticalAngle, CUSTOM_GRAVITY_MAX_HORIZONTAL_ANGLE);
                    float ratio = (anglesPair.angle / anglesPair.maxAngle);

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
