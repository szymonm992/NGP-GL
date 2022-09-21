using UnityEngine;
using System;
using Frontend.Scripts.Enums;

namespace Frontend.Scripts.Models
{
    [Serializable]
    public class UTAxleDebug
    {
        [Header("Gizmos")]
        [SerializeField] private bool drawGizmos;
        [SerializeField] private bool drawAxleCenter;
        [SerializeField] private bool drawAxlePipes;
        [SerializeField] private UTDebugMode drawMode;

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

        public bool DrawAxleCenter
        {
            get => drawAxleCenter;
            set
            {
                this.drawAxleCenter = value;
            }
        }


        public bool DrawAxlePipes
        {
            get => drawAxlePipes;
            set
            {
                this.drawAxlePipes = value;
            }
        }

    }
}
