using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Frontend.Scripts.Components
{
    public class TestSuspension : MonoBehaviour
    {
        [SerializeField] private HoverSpring[] allWheels;
        [SerializeField] private Rigidbody rig;
        [SerializeField] private float driveForce;

        private float inputX, inputY;
        private float absoluteInputY, absoluteInputX;

        private Vector3 wheelVelocityLocal;
        private float Fx, Fy;
        public bool isBrake;
        public float currentLongitudalGrip;
        public HoverSpring[] AllWheels => allWheels;

        private void Update()
        {
            isBrake = Input.GetKey(KeyCode.Space);
            inputY = !isBrake ? Input.GetAxis("Vertical") : 0f;
            inputX = Input.GetAxis("Horizontal");

            absoluteInputY = Mathf.Abs(inputY);
            absoluteInputX = Mathf.Abs(inputX);

            foreach (var wheel in allWheels)
            {
                wheel.inputX = inputX;
                wheel.inputY = inputY;
                wheel.absoluteInputX = absoluteInputX;
                wheel.absoluteInputY = absoluteInputY;
            }
        }
        private void FixedUpdate()
        {
            //Accelerate();
           // Brakes();
        }
        /*
        private void Accelerate()
        {
            foreach (var wheel in allWheels)
            {
                if (wheel.canDrive & wheel.IsGrounded && !isBrake)
                {
                    wheelVelocityLocal = transform.InverseTransformDirection(rig.GetPointVelocity(wheel.hit.point));

                    Fx = inputY * driveForce/2;
                    Fy = wheelVelocityLocal.x * driveForce;

                    rig.AddForceAtPosition((Fx * wheel.transform.forward) + (Fy * -wheel.transform.right), wheel.hit.point);
                }

            }

        }

        private void Brakes()
        {
             currentLongitudalGrip = isBrake ? 1f : (absoluteInputY > 0 ? 0 : 0.5f);
            foreach (var wheel in allWheels)
            {
               
                if (wheel.IsGrounded)
                {
                    Vector3 forwardDir = wheel.transform.forward;
                    Vector3 tireVel = rig.GetPointVelocity(wheel.transform.position);

                    float steeringVel = Vector3.Dot(forwardDir, tireVel);
                    float desiredVelChange = -steeringVel * currentLongitudalGrip;
                    float desiredAccel = desiredVelChange / Time.fixedDeltaTime;

                    rig.AddForceAtPosition(forwardDir * wheel.spring.tireMass * desiredAccel, wheel.transform.position);
                }
            }

        }*/
    }
}
