using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Frontend.Scripts.Models
{
    [System.Serializable]
    public class DamperInfo
    {
        [SerializeField] private float unitBumpForce = 400f;
        [SerializeField] private float unitReboundForce = 1800f;
        [SerializeField] private float damperForce = 2000f;

        private float finalForce;
        private float maxForce;

        public float UnitBumpForce => unitBumpForce;
        public float UnitReboundForce => unitReboundForce;
        public float DamperForce => damperForce;
        public float MaxForce
        {
            get => maxForce;
            set { this.maxForce = value; }
        }
        public float FinalForce
        {
            get => finalForce;
            set { this.finalForce = value; }
        } 
           

    }
}
