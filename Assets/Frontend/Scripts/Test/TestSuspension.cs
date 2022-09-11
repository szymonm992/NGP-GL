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
        private Vector3 wheelVelocityLocal;
        private float Fx, Fy;
        private bool isBrake;

        public TestWheel[] AllWheels => allWheels;

        private void Update()
        {
            isBrake = Input.GetKey(KeyCode.Space);
            inputY = !isBrake ? Input.GetAxis("Vertical") : 0f;
        }
        private void FixedUpdate()
        {
            Accelerate();
        }

        private void Accelerate()
        {
            foreach(TestWheel wheel in allWheels)
            {
                if(wheel.canDrive & wheel.IsGrounded)
                {
                    wheelVelocityLocal = transform.InverseTransformDirection(rig.GetPointVelocity(wheel.Hit.point));

                    Fx = inputY * wheel.SpringForce/2;
                    Fy = wheelVelocityLocal.x * wheel.SpringForce;

                    rig.AddForceAtPosition((Fx * wheel.transform.forward) + (Fy * -wheel.transform.right), wheel.Hit.point);
                }
               
            }
            
        }
    }
}
