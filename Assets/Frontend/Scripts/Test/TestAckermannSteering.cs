using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Frontend.Scripts.Components
{
    public class TestAckermannSteering : MonoBehaviour
    {

        [SerializeField] private TestSuspension suspensionController;

        [SerializeField] private float wheelBase;
        [SerializeField] private float rearTrack;
        [SerializeField] private float turnRadius;
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

            
            foreach(var wheel in suspensionController.AllWheels)
            {
                if(wheel.canSteer)
                {
                    if (wheel.isLeft)
                    {
                        wheel.SteerAngle = ackermannAngleLeft;
                    }
                    else
                    {
                        wheel.SteerAngle = ackermannAngleRight;
                    }
                }
            }
        }
    }
}
