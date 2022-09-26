using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Frontend.Scripts.Components
{
    public class AntiRollBar : MonoBehaviour
    {
        [SerializeField] private Rigidbody rig;
        [SerializeField] private AnimationCurve antirollCurve;
        [SerializeField] private UTSuspensionController suspensionController;

        public UTWheel wheelL;
        public UTWheel wheelR;
        public float antiRollVal = 300f;
        
        public float travelL = 0f;
        public float travelR = 0f;
        void Update()
        {
           
            bool groundedL = wheelL.IsGrounded;
            if (groundedL)
            {
                travelL = wheelL.CompressionRate;
            }
            else
            {
                travelL = 1f;
            }
            bool groundedR = wheelR.IsGrounded;
            if (groundedR)
            {
                travelR = wheelR.CompressionRate;
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
