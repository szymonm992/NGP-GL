using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Frontend.Scripts
{
    public class Test : MonoBehaviour
    {
        Rigidbody rig;

        public float restLength, springTravel, springStiffness;

        private float minLength, maxLength, springLength, springForce;

        private Vector3 suspensionForce;

        public float wheelRadius;


        public CustomWheel[] wheels;


        void Start()
        {
            rig = GetComponent<Rigidbody>();

            minLength = restLength - springTravel;
            minLength = restLength + springTravel;
        }

        private void FixedUpdate()
        {
            foreach(CustomWheel wheel in wheels)
            {
                if (wheel.IsColliding)
                {
                    springLength = DistanceFromWheelC(wheel, wheel.allCollisions[0].contacts[0].point) - wheelRadius;

                    springForce = springStiffness * (restLength - springLength);

                    suspensionForce = springForce * transform.up;

                    rig.AddForceAtPosition(suspensionForce, wheel.allCollisions[0].contacts[0].point);
                }
            }
            
        }

        public float DistanceFromWheelC(CustomWheel cust, Vector3 point)
        {
            return Vector3.Distance(point, cust.transform.position);
        }
    }
}
