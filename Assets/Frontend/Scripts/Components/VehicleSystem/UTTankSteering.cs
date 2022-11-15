using Frontend.Scripts.Interfaces;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Frontend.Scripts
{
    public class UTTankSteering : MonoBehaviour, IVehicleSteering
    {
        [Inject(Id = "mainRig")] private Rigidbody rig;
        [Inject] private readonly IVehicleController suspensionController;
        [Inject] private readonly IPlayerInputProvider inputProvider;

        [SerializeField] private float steerForce;
        private float steerInput;
        private float currentSteerSpeed;

        public float SteerForce => steerForce;

        public void SetSteeringInput(float input)
        {
            steerInput = inputProvider.AbsoluteVertical != 0 ?  input * inputProvider.SignedVertical : input;
        }

        private void Update()
        {
            SetSteeringInput(inputProvider.Horizontal);
        }


        private void FixedUpdate()
        {
            if (steerInput == 0 || suspensionController.IsUpsideDown)
            {
                return;
            }

            currentSteerSpeed = steerForce;

            if (inputProvider.CombinedInput > 1)
            {
                currentSteerSpeed *= (1.0f / Mathf.Sqrt(2));
            }

            foreach(var axle in suspensionController.AllAxles)
            {
                if(axle.CanSteer)
                {
                    var wheelsInAxle = axle.AllWheels;
                    foreach(var wheel in wheelsInAxle)
                    {
                        if(wheel.IsGrounded)
                        {
                            int invert = axle.SteeringInverted ? -1 : 1;
                            rig.AddForceAtPosition(rig.mass * steerForce * rig.transform.right * steerInput * invert, wheel.HitInfo.Point);
                        }
                        
                    }
                }
            }
            /*
            if (Mathf.Abs(rig.angularVelocity.y) < 1f)
            {
                rig.AddRelativeTorque(Vector3.up * steerInput * currentSteerSpeed * 50f * Time.deltaTime, ForceMode.Acceleration);
            }*/

        }
    }
}
