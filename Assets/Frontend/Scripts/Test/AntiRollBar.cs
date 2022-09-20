using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Frontend.Scripts.Components
{
    public class AntiRollBar : MonoBehaviour
    {
        [SerializeField] private Rigidbody rig;
        [SerializeField] private AnimationCurve antirollCurve;
        [SerializeField] private TestSuspension suspensionController;

        public HoverSpring wheelL;
        public HoverSpring wheelR;
        public float antiRollVal = 300f;
        
        public float travelL = 0f;
        public float travelR = 0f;
        void Update()
        {
           
            bool groundedL = wheelL.IsGrounded;
            if (groundedL)
            {
                travelL = (-wheelL.transform.InverseTransformPoint(wheelL.HitInfo.Point).y - wheelL.WheelRadius) / wheelL.SpringInfo.SuspensionLength;
            }
            else
            {
                travelL = 1f;
            }
            bool groundedR = wheelR.IsGrounded;
            if (groundedR)
            {
                travelR = (-wheelR.transform.InverseTransformPoint(wheelR.HitInfo.Point).y - wheelR.WheelRadius) / wheelR.SpringInfo.SuspensionLength;
            }
            else
            {
                travelL = 1f;
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
