using UnityEngine;
using System;
using Frontend.Scripts.Enums;

namespace Frontend.Scripts.Models
{
 
    [Serializable]
    public class UnderWheelDebug
    {
        [Header("Gizmos")]
        [SerializeField] private bool drawGizmos;
        [SerializeField] private UnderWheelDebugMode drawMode;

        [Header("Force")]
        [SerializeField] private bool drawForce;

        public UnderWheelDebugMode DrawMode
        {
            get => drawMode;
            set
            {
                this.drawMode = value;
            }
        }
        public bool DrawGizmos
        {
            get => drawGizmos;
            set
            {
                this.drawGizmos = value;
            }
        }

        public bool DrawForce
        {
            get => drawForce;
            set
            {
                this.drawForce = value;
            }
        }
    }
}
