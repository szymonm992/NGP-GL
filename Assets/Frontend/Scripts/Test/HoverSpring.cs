using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Frontend.Scripts.Models;
using Frontend.Scripts.Enums;
using UnityEditor;

namespace Frontend.Scripts.Components
{
	public class HoverSpring : MonoBehaviour
	{
		[SerializeField] private float wheelRadius = 0.6f;
		[SerializeField] private float tireMass = 30f;
		[Range(0, 1f)]
		[SerializeField] private float forwardTireGripFactor = 1f, sidewaysTireGripFactor = 1f;
		[SerializeField] private SpringInfo springInfo;
		[SerializeField] private LayerMask layerMask;

		
		private HitInfo hitInfo = new HitInfo();
		private float compressionLength;

		private Rigidbody rig;
		
		private bool isGrounded = false;
		private float wheelAngle = 0f;
		private float steerAngle;
		private float springAndCenterDistance;
		private Vector3 tireWorldPosition;

		public bool canDrive = true;
		public bool isLeft = false;
		public bool canSteer = false;

		[SerializeField]
		private UTWheelDebug debugSettings = new UTWheelDebug()
		{
			DrawGizmos = true,
			DrawOnDisable = false,
			DrawMode = UTDebugMode.All,
			DrawForce = true
		};

		public SpringInfo SpringInfo => springInfo;
		public HitInfo HitInfo => hitInfo;
		public float CompressionLength => compressionLength;
		public float WheelRadius => wheelRadius;
		public float ForwardTireGripFactor => forwardTireGripFactor;
		public float SidewaysTireGripFactor => sidewaysTireGripFactor;
		public float TireMass => tireMass;
		public bool IsGrounded => isGrounded;

		public Vector3 GetTireWorldPosition => tireWorldPosition;

		public float SteerAngle
        {
			get => steerAngle;
            set { this.steerAngle = value; }
        }

        private void OnValidate()
        {
			springAndCenterDistance = springInfo.SuspensionLength;
		}

        private void Awake()
		{
			rig = transform.root.GetComponent<Rigidbody>();
		}

		private void FixedUpdate()
		{
			HitUpdate();
			ForceUpdate();
		}

		private void Update()
		{
			wheelAngle = Mathf.Lerp(wheelAngle, steerAngle, Time.deltaTime * 8f);
			transform.localRotation = Quaternion.Euler(transform.localRotation.x,
				transform.localRotation.y + wheelAngle,
				transform.localRotation.z);
		}

		private void HitUpdate()
		{
			
			//isGrounded = Physics.Raycast(transform.position, -transform.up, out hitInfo.rayHit, springInfo.SuspensionLength);
			isGrounded = Physics.SphereCast(transform.position,wheelRadius, -transform.up, out hitInfo.rayHit, springInfo.SuspensionLength, layerMask);
	
			if (IsGrounded)
			{
				hitInfo.forwardDir = Vector3.Normalize(Vector3.Cross(hitInfo.Normal, -transform.right));
				hitInfo.sidewaysDir = Quaternion.AngleAxis(90f, hitInfo.Normal) * hitInfo.forwardDir;
				if(debugSettings.DrawForce)
                {
					Debug.DrawRay(hitInfo.Point, hitInfo.forwardDir, Color.blue);
					Debug.DrawRay(hitInfo.Point, hitInfo.sidewaysDir, Color.magenta);
				}
				springAndCenterDistance = hitInfo.Distance;
			}
			else
            {
				springAndCenterDistance = springInfo.SuspensionLength;
			}

			tireWorldPosition = transform.position - transform.up * springAndCenterDistance;
		}
		
		private void ForceUpdate()
		{
			if (!IsGrounded)
			{
				return;
			}

			Vector3 springDir = transform.up;
			Vector3 tireWorldVel = rig.GetPointVelocity(transform.position);
			float offset = springInfo.SuspensionLength - hitInfo.Distance;
			float vel = Vector3.Dot(springDir, tireWorldVel);

			float force = (springInfo.SpringStrength * offset) - (vel * springInfo.DamperForce);
			rig.AddForceAtPosition(springDir * force, transform.position);
		}

        private void OnDrawGizmos()
        {
			if(!enabled)
            {
				return;
            }
			
			Gizmos.DrawSphere(transform.position, 0.1f);
			Gizmos.DrawSphere(transform.position - (transform.up * springAndCenterDistance), 0.1f);
			Handles.color = Color.white;

			if (isGrounded)
            {
				Gizmos.color = Color.white;
				Gizmos.DrawSphere(hitInfo.Point, 0.1f);
				Handles.DrawLine(transform.position, transform.position - (transform.up * springAndCenterDistance), 1.6f);
			}
			else
            {
				Gizmos.color = Color.red;
				Handles.DrawLine(transform.position, transform.position - (transform.up * springAndCenterDistance), 1.6f);
			}

			Gizmos.DrawWireSphere(transform.position - transform.up * (springAndCenterDistance), wheelRadius);
			
		}
    }
}
