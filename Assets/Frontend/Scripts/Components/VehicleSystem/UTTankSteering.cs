using Frontend.Scripts.Interfaces;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
using GLShared.General.Interfaces;
using GLShared.General.Components;

namespace Frontend.Scripts.Components
{
    public class UTTankSteering : MonoBehaviour, IVehicleSteering
    {
        private const float OVERREACHED_MAX_ANGLE_TURNING_MULTIPLIER  = 0.25f;

        [Inject(Id = "mainRig")] private Rigidbody rig;
        [Inject] private readonly IVehicleController suspensionController;
        [Inject] private readonly IPlayerInputProvider inputProvider;

        [SerializeField] private float steerForce;

        private float steerInput;
        private float currentSteerForce;

        public float SteerForce => steerForce;

        public void SetSteeringInput(float input)
        {
            steerInput = inputProvider.AbsoluteVertical != 0 ? input * inputProvider.SignedVertical : input;
        }

        private void Update()
        {
            if (suspensionController != null)
            {
                SetSteeringInput(inputProvider.Horizontal);
            }
        }

        private void FixedUpdate()
        {
            if (steerInput == 0 || suspensionController.IsUpsideDown )
            {
                return;
            }
             
            currentSteerForce = GetSteerForceMultiplier();

            if (inputProvider.CombinedInput > 1f)
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
                        }
                    }
                }
            }
        }

        private float GetSteerForceMultiplier()
        {
            var currentPair = suspensionController.CurrentFrictionPair;

            if (suspensionController.HorizontalAngle >= currentPair.HorizontalAnglesRange.Max)
            {
                return 0.1f;
            }

            return currentPair.IsDefaultLayer ? steerForce : steerForce * currentPair.SteeringMultiplier;
        }
    }
}
