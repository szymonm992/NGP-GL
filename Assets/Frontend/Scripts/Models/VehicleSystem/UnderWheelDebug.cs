using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
namespace Frontend.Scripts.Models
{
 
    [Serializable]
    public class UnderWheelDebug
    {
       [SerializeField] private bool drawGizmos;
       [SerializeField] private UnderWheelDebugMode drawMode;

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
    }
}
