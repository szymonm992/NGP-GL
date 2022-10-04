using UnityEngine;

namespace Frontend.Scripts.ScriptableObjects
{
    [CreateAssetMenu(fileName = "GameParameters", menuName = "UT/Game settings/GameParameters")]
    public class GameParameters : ScriptableObject
    {
        [Header("Vehicles wheel parameters")]
        [SerializeField] private float maxWheelDetectionAngle = 75f;

        [Header("Vehicles air control")]
        [SerializeField] private float airControlAngleThreshold = 25f;
        [SerializeField] private float airControlForce = 6f;

        public float MaxWheelDetectionAngle => maxWheelDetectionAngle;
        public float AirControlAngleThreshold => airControlAngleThreshold;
        public float AirControlForce => airControlForce;
    }
}
