using Frontend.Scripts.Interfaces;
using Sfs2X.Bitswarm;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Frontend.Scripts.Models
{
    public abstract class WheelRepositionBase : MonoBehaviour, IWheelReposition
    {
        public virtual void RotateWheels()
        {
        }
    }
}
