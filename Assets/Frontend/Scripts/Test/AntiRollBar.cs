using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Frontend.Scripts.Components
{
    public class AntiRollBar : MonoBehaviour
    {
        [SerializeField] private Rigidbody rig;
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
                travelL = (-wheelL.transform.InverseTransformPoint(wheelL.HitInfo.Point).y - 0.5f) / wheelL.CompressionLength;
            }
            bool groundedR = wheelR.IsGrounded;
            if (groundedR)
            {
                travelR = (-wheelR.transform.InverseTransformPoint(wheelR.HitInfo.Point).y - 0.5f) / wheelR.CompressionLength;
            }

            float antiRollForce = (travelL - travelR) * antiRollVal;

            if (!groundedL)
            {
                rig.AddForceAtPosition(wheelL.transform.up * -antiRollForce,
                       wheelL.transform.position);
            }
            if (!groundedR)

            {
                rig.AddForceAtPosition(wheelR.transform.up * antiRollForce,

                      wheelR.transform.position);
            }
        }
    }
}
