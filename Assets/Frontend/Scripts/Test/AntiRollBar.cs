using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Frontend.Scripts.Components
{
    public class AntiRollBar : MonoBehaviour
    {
        [SerializeField] private Rigidbody rig;
        public TestWheel wheelL;
        public TestWheel wheelR;
        public float antiRollVal = 5000f;

        void Update()
        {
            float travelL = 1.0f;
            float travelR = 1.0f;
            bool groundedL = wheelL.IsGrounded;
            if (groundedL)
            {
                travelL = (-wheelL.transform.InverseTransformPoint(wheelL.Hit.point).y - wheelL.WheelRadius) / wheelL.SpringLength;
            }
            bool groundedR = wheelR.IsGrounded;
            if (groundedR)
            {
                travelR = (-wheelR.transform.InverseTransformPoint(wheelR.Hit.point).y - wheelR.WheelRadius) / wheelR.SpringLength;


            }

            float antiRollForce = (travelL - travelR) * antiRollVal;



            if (groundedL)

                rig.AddForceAtPosition(wheelL.transform.up * -antiRollForce,

                       wheelL.transform.position);

            if (groundedR)

                rig.AddForceAtPosition(wheelR.transform.up * antiRollForce,

                       wheelR.transform.position);
        }
    }
}
