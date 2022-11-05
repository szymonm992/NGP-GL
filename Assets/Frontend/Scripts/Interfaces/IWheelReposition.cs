using Frontend.Scripts.Models;
using UnityEngine;

namespace Frontend.Scripts.Interfaces
{
    public interface IWheelReposition 
    {
        public abstract void RotateWheels(float verticalDir, Vector3 rotateAroundAxis, Transform tireTransform, UTAxlePair pair, out float currentToMaxRatio);

        void TrackMovement(Transform tireTransform, UTAxlePair pair, Vector3 finalWheelPosition, float trackMovementSpeed);
    }
}
