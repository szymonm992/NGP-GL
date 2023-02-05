using Frontend.Scripts.Interfaces;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
using GLShared.General.Interfaces;

namespace Frontend.Scripts.Components
{
    public class UTTankSteering : MonoBehaviour, IVehicleSteering
    {
        [Inject(Id = "mainRig")] private Rigidbody rig;
        [Inject] private readonly IVehicleController suspensionController;
        [Inject] private readonly IPlayerInputProvider inputProvider;

        [SerializeField] private float steerForce;

        private float steerInput;
        private float currentSteerForce;

        public float SteerForce => steerForce;

        public void SetSteeringInput(float input)
        {
            steerInput = inputProvider.AbsoluteVertical != 0 ?  input * inputProvider.SignedVertical : input;
        }

        private void Update()
        {
            if(suspensionController != null)
            {
                SetSteeringInput(inputProvider.Horizontal);
            }
        }

        private void FixedUpdate()
        {
            if (steerInput == 0 || suspensionController.IsUpsideDown)
            {
                return;
            }

            currentSteerForce = steerForce;

            if (inputProvider.CombinedInput > 1)
            {
                currentSteerForce *= (1.0f / Mathf.Sqrt(2));
            }

            if (!suspensionController.RunPhysics)
            {
                return;
            }

            foreach (var axle in suspensionController.AllAxles)
            {
                if(axle.CanSteer)
                {
                    var wheelsInAxle = axle.AllWheels;

                    foreach(var wheel in wheelsInAxle)
                    {
                        int invertValue = axle.InvertSteer ? -1 : 1;
                        if (wheel.IsGrounded)
                        {
                            float idlerMultiplier = wheel.IsIdler ? 0.3f : 1f;
                            rig.AddForceAtPosition(invertValue  * currentSteerForce * idlerMultiplier * steerInput * rig.transform.right,
                                wheel.HitInfo.Point, ForceMode.Force);

                            /*
                            var slopeFactor = Vector3.Dot(rig.velocity, Physics.gravity.normalized);
                            Vector3 turnTorque = transform.up * (invertValue * steerInput) * currentSteerForce *  (1f - 0.5f * Mathf.Abs(slopeFactor));
                            rig.AddTorque(turnTorque, ForceMode.VelocityChange);*/


                            /*
                            float angleFactor = suspensionController.HorizontalAngle > 15f ? (0.8f * suspensionController.HorizontalAngle / 15f) : 0f;
                            var slopeFactor = Vector3.Dot(transform.up, Physics.gravity.normalized);
                            float turnSpeedModified = currentSteerForce * (1f - Mathf.Abs(slopeFactor * angleFactor));
                            Vector3 turnTorque = transform.up * (invertValue * steerInput) * turnSpeedModified;
                            rig.AddTorque(turnTorque, ForceMode.VelocityChange);
                            return;*/

                            /*
                            float slopeFactor = Vector3.Dot(transform.up, Physics.gravity.normalized);
                            float torque = ( steerInput) * currentSteerForce * slopeFactor * Time.fixedDeltaTime;
                            rig.AddTorque(transform.up * torque, ForceMode.VelocityChange);
                            return;*/
                        }
                    }
                }
            }
        }
    }
}
