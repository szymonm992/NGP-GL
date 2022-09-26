using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Frontend.Scripts.Components
{
    public class AntiRollBar : MonoBehaviour
    {
        [Inject] private Rigidbody rig;

        public UTWheel wheelL;
        public UTWheel wheelR;
        public float antiRollVal = 3000f;

        private float antiRollForce = 0;
        void Update()
        {
            antiRollForce = (wheelL.CompressionRate - wheelR.CompressionRate) * antiRollVal;

       
                rig.AddForceAtPosition(wheelL.transform.up * antiRollForce,
                       wheelL.transform.position);

        
                rig.AddForceAtPosition(wheelR.transform.up * -antiRollForce,

                      wheelR.transform.position);
            
        }
    }
}
