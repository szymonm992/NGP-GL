using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Frontend.Scripts.Components
{
    public class TestSuspension : MonoBehaviour
    {
        [SerializeField] private TestWheel[] allWheels;
        [SerializeField] private Rigidbody rig;

        private float inputX, inputY;
        private float absoluteInputY, absoluteInputX;

        private Vector3 wheelVelocityLocal;
        private float Fx, Fy;
        public bool isBrake;
        public float currentLongitudalGrip;
        public TestWheel[] AllWheels => allWheels;

        private void Update()
        {
            isBrake = Input.GetKey(KeyCode.Space);
            inputY = !isBrake ? Input.GetAxis("Vertical") : 0f;
            inputX = Input.GetAxis("Horizontal");

            absoluteInputY = Mathf.Abs(inputY);
            absoluteInputX = Mathf.Abs(inputX);
        }
        private void FixedUpdate()
        {
            Accelerate();
            Brakes();
        }

        private void Accelerate()
        {
            foreach (TestWheel wheel in allWheels)
            {
                if (wheel.canDrive & wheel.IsGrounded && !isBrake)
                {
                    wheelVelocityLocal = transform.InverseTransformDirection(rig.GetPointVelocity(wheel.Hit.point));

                    Fx = inputY * wheel.SpringForce / 2;
                    Fy = wheelVelocityLocal.x * wheel.SpringForce;

                    rig.AddForceAtPosition((Fx * wheel.transform.forward) + (Fy * -wheel.transform.right), wheel.Hit.point);
                }

            }

        }

        private void Brakes()
        {
             currentLongitudalGrip = isBrake ? 1f : (absoluteInputY > 0 ? 0 : 0.5f);
            foreach (TestWheel wheel in allWheels)
            {
               
                if (wheel.IsGrounded)
                {
                    Vector3 forwardDir = wheel.transform.forward;
                    Vector3 tireVel = rig.GetPointVelocity(wheel.transform.position);

                    float steeringVel = Vector3.Dot(forwardDir, tireVel);
                    float desiredVelChange = -steeringVel * currentLongitudalGrip;
                    float desiredAccel = desiredVelChange / Time.fixedDeltaTime;

                    rig.AddForceAtPosition(forwardDir * wheel.TireMass * desiredAccel, wheel.transform.position);
                }
            }

        }
    }
}
