using Frontend.Scripts.Models;
using UnityEngine;

namespace Frontend.Scripts.Interfaces
{
    public interface IWheelReposition 
    {
        float RepositionSpeed { get; }
        public abstract void RotateWheel(float verticalDir, Vector3 rotateAroundAxis, Transform tireTransform, UTAxlePair pair);

        void DummiesMovement(Transform tireTransform, UTAxlePair pair, Vector3 finalWheelPosition, float movementSpeed);

    }
}
