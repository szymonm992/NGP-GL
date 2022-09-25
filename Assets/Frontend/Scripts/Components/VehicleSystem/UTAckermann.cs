using Frontend.Scripts.Enums;
using Frontend.Scripts.Interfaces;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Frontend.Scripts
{
    public class UTAckermann : MonoBehaviour, IVehicleSteering
    {
        [Inject] private readonly IVehicleController suspensionController;

        [SerializeField] private float wheelBase;
        [SerializeField] private float rearTrack;
        [SerializeField] private AnimationCurve turnRadiusCurve;

        private float ackermannAngleRight;
        private float ackermannAngleLeft;
        private float steerInput;
        private float currentTurnRadius;

        private void Update()
        {
            steerInput = Input.GetAxis("Horizontal");

            currentTurnRadius = turnRadiusCurve.Evaluate(suspensionController.CurrentSpeed);

            if (steerInput > 0)
            {
                ackermannAngleLeft = Mathf.Rad2Deg * Mathf.Atan(wheelBase / (currentTurnRadius + (rearTrack / 2))) * steerInput;
                ackermannAngleRight = Mathf.Rad2Deg * Mathf.Atan(wheelBase / (currentTurnRadius - (rearTrack / 2))) * steerInput;
            }
            else if (steerInput < 0)
            {
                ackermannAngleLeft = Mathf.Rad2Deg * Mathf.Atan(wheelBase / (currentTurnRadius - (rearTrack / 2))) * steerInput;
                ackermannAngleRight = Mathf.Rad2Deg * Mathf.Atan(wheelBase / (currentTurnRadius + (rearTrack / 2))) * steerInput;
            }
            else
            {
                ackermannAngleLeft = 0;
                ackermannAngleRight = 0;
            }

            if(suspensionController.HasAnyWheels)
            {
                foreach (var axle in suspensionController.AllAxles)
                {
                    if (axle.CanSteer)
                    {
                        axle.SetSteerAngle(ackermannAngleLeft, ackermannAngleRight);
                    }
                }
            }
            
        }
    }
}
