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

        private float ackermannAngleRight;
        private float ackermannAngleLeft;
        private float steerInput;

        private void Update()
        {
            steerInput = Input.GetAxis("Horizontal");

            if (steerInput > 0)
            {
                ackermannAngleLeft = Mathf.Rad2Deg * Mathf.Atan(wheelBase / (turnRadius + (rearTrack / 2))) * steerInput;
                ackermannAngleRight = Mathf.Rad2Deg * Mathf.Atan(wheelBase / (turnRadius - (rearTrack / 2))) * steerInput;
            }
            else if (steerInput < 0)
            {
                ackermannAngleLeft = Mathf.Rad2Deg * Mathf.Atan(wheelBase / (turnRadius - (rearTrack / 2))) * steerInput;
                ackermannAngleRight = Mathf.Rad2Deg * Mathf.Atan(wheelBase / (turnRadius + (rearTrack / 2))) * steerInput;
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
                        wheel.steerAngle = ackermannAngleLeft;
                    }
                    else
                    {
                        wheel.steerAngle = ackermannAngleRight;
                    }
                }
            }
        }
    }
}
