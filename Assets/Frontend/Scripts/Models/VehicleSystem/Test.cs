using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Frontend.Scripts
{
    public class Test : MonoBehaviour
    {
        Rigidbody rig;

        public float restLength, springTravel, springStiffness,damperStiffness;

        private float minLength, maxLength,lastLength, springLength, springForce,damperForce,springVelocity;

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
                    
                        /*lastLength = springLength;
                        springLength = DistanceFromWheelC(wheel, cp.point) - wheelRadius;
                        springLength = Mathf.Clamp(springLength, minLength, maxLength);
                        springVelocity = (lastLength - springLength) / Time.fixedDeltaTime;

                        springForce = springStiffness * (restLength - springLength);


                        damperForce = damperStiffness * springVelocity;
                        suspensionForce = (springForce + damperForce) * transform.up;

                        rig.AddForceAtPosition(suspensionForce, wheel.allCollisions[0].contacts[0].point);*/

                    


                }
            }

            
        }

        public float DistanceFromWheelC(CustomWheel cust, Vector3 point)
        {
            return Vector3.Distance(point, cust.transform.position);
        }
    }
}
