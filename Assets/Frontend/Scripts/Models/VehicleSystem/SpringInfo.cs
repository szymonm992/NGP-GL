using UnityEngine;

namespace Frontend.Scripts.Models
{
	[System.Serializable]
    public class SpringInfo
    {
		[SerializeField] private float springStrength = 30000f;
		[SerializeField] private float suspensionLength = 1.3f;
		[Range(0, 1f)]
		[SerializeField] private float tireGripFactor = 1f;
		[SerializeField] private float tireMass = 20f;
		[SerializeField] private float damperForce = 2000f;

		public float DamperForce => damperForce;
		public float SpringStrength => springStrength;
		public float SuspensionLength => suspensionLength;
		public float TireGripFactor => tireGripFactor;
		public float TireMass => tireMass;
	}
}
