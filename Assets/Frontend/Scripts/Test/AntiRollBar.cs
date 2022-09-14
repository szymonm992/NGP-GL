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
        public float antiRollVal = 5000f;

        void Update()
        {
            float travelL = 1.0f;
            float travelR = 1.0f;
            bool groundedL = wheelL.grounded;
            if (groundedL)
            {
                travelL = (-wheelL.transform.InverseTransformPoint(wheelL.hit.point).y - 0.3f) / wheelL.spring.compressionLength;
            }
            bool groundedR = wheelR.grounded;
            if (groundedR)
            {
                travelR = (-wheelR.transform.InverseTransformPoint(wheelR.hit.point).y - 0.3f) / wheelR.spring.compressionLength;


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
