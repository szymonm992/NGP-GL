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
        [Inject] protected readonly IVehicleController controller;
        [Inject] protected readonly IPlayerInputProvider inputProvider;

        public virtual void RotateWheels(float verticalDir, Vector3 rotateAroundAxis, Transform tireTransform, UTAxlePair pair, out float currentToMaxRatio)
        {
            currentToMaxRatio = 0f;
        }

        public virtual void TrackMovement(Transform tireTransform, UTAxlePair pair, Vector3 finalWheelPosition)
        {
        }
    }
}
