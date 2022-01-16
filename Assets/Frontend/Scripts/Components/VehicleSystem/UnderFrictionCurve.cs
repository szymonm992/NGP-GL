using UnityEngine;
using System;

namespace Frontend.Scripts.Models
{
    [Serializable]
    public class UnderFrictionCurve
    {
        
        [SerializeField] private float extremumSlip;
        [SerializeField] private float extremumValue;
        [SerializeField] private float asymptoteSlip;
        [SerializeField] private float asymptoteValue;
        [SerializeField] private float stiffness;

        public float AsymptoteSlip
        {
            get { return this.asymptoteSlip; }
            set { this.asymptoteSlip = value; }
        }

        public float AsymptoteValue
        {
            get { return this.asymptoteValue; }
            set { this.asymptoteValue = value; }
        }

        public float ExtremumSlip
        {
            get { return this.extremumSlip; }
            set { this.extremumSlip = value;  }
        }

        public float ExtremumValue
        {
            get { return this.extremumValue;  }
            set { this.extremumValue = value; }
        }

        public float Stiffness
        {
            get { return this.stiffness; }
            set { this.stiffness = value; }
        }

        public float Evaluate(float currentSlip)
        {
            float traction = asymptoteValue;
            float currentAbsoluteSlip = Mathf.Abs(currentSlip);

            if(currentAbsoluteSlip <= extremumSlip)
            {
                traction = (extremumValue / extremumSlip) * currentAbsoluteSlip;
            }
            else if(currentAbsoluteSlip > extremumSlip && currentAbsoluteSlip < asymptoteSlip)
            {
                traction = 
                    ((asymptoteValue - extremumValue) / (asymptoteSlip - extremumSlip)) * (currentAbsoluteSlip - extremumSlip) + extremumValue;
            }
            return traction * stiffness;
        }
    }
}
