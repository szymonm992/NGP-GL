using UnityEngine;
using System;
using Frontend.Scripts.Enums;

namespace Frontend.Scripts.Models
{
 
    [Serializable]
    public class UTWheelDebug
    {
        [Header("Gizmos")]
        [SerializeField] private bool drawGizmos;
        [SerializeField] private bool drawOnDisable;
        [SerializeField] private bool drawWheelDirection;
        [SerializeField] private bool drawSphereGizmo;
        [SerializeField] private bool drawSprings;
        [SerializeField] private UTDebugMode drawMode;

        [Header("Force")]
        [SerializeField] private bool drawForce;

        public UTDebugMode DrawMode
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
        
        public bool DrawOnDisable
        {
            get => drawOnDisable;
            set
            {
                this.drawOnDisable = value;
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

        public bool DrawWheelDirection
        {
            get => drawWheelDirection;
            set
            {
                this.drawWheelDirection = value;
            }
        }
        public bool DrawSphereGizmo
        {
            get => drawSphereGizmo;
            set
            {
                this.drawSphereGizmo = value;
            }
        }
        public bool DrawSprings
        {
            get => drawSprings;
            set
            {
                this.drawSprings = value;
            }
        }
    }
}
