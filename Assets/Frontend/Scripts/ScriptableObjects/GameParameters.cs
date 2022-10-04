using UnityEngine;

namespace Frontend.Scripts.ScriptableObjects
{
    [CreateAssetMenu(fileName = "GameParameters", menuName = "UT/Game settings/GameParameters")]
    public class GameParameters : ScriptableObject
    {
        [SerializeField] private float maxWheelDetectionAngle = 75f;
        [SerializeField] private float airControlAngleThreshold = 8f;

        public float MaxWheelDetectionAngle => maxWheelDetectionAngle;
        public float AirControlAngleThreshold => airControlAngleThreshold;
    }
}
