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
		

		[SerializeField] private float wheelRadius = 0.33f;
		[SerializeField] private SpringInfo springInfo;
		[SerializeField] private DamperInfo damperInfo;
		[SerializeField] private LayerMask layerMask;
		

		private HitInfo hitInfo;
		private float force;
		private float velocity;
		private float preLength, length;
		private float compressionLength;
		private float preOverflow, overflow, overflowVelocity;
		private bool bottomedOut, overExtended;
		private float bottomedOutForce;

		private Rigidbody rig;
		
		private bool isGrounded = false;
		private float wheelAngle = 0f;
		private float steerAngle;
		private float springAndCenterDistance;
		private Vector3 totalForce;

		public bool canDrive = true;
		public bool isLeft = false;
		public bool canSteer = false;

		[SerializeField]
		private UnderWheelDebug debugSettings = new UnderWheelDebug()
		{
			DrawGizmos = true,
			DrawOnDisable = false,
			DrawMode = UnderWheelDebugMode.All,
			DrawForce = true
		};

		public SpringInfo SpringInfo => springInfo;
		public DamperInfo DamperInfo => damperInfo;
		public HitInfo HitInfo => hitInfo;
		public float CompressionLength => compressionLength;
		public bool IsGrounded => isGrounded;

		public float SteerAngle
        {
			get => steerAngle;
            set { this.steerAngle = value; }
        }



		private void Awake()
		{
			rig = transform.root.GetComponent<Rigidbody>();
			hitInfo = new HitInfo();
		}

		private void FixedUpdate()
		{
			HitUpdate();
			SuspensionUpdate();
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
			isGrounded = Physics.Raycast(transform.position, -transform.up, out hitInfo.rayHit, springInfo.SuspensionLength);
			//isGrounded = Physics.SphereCast(transform.position,wheelRadius, -transform.up, out hitInfo.rayHit, springInfo.SuspensionLength, layerMask);
	
			if (IsGrounded)
			{
				hitInfo.forwardDir = Vector3.Normalize(Vector3.Cross(hitInfo.Normal, -transform.right));
				hitInfo.sidewaysDir = Quaternion.AngleAxis(90f, hitInfo.Normal) * hitInfo.forwardDir;
				if(debugSettings.DrawForce)
                {
					Debug.DrawRay(hitInfo.Point, hitInfo.forwardDir, Color.blue);
					Debug.DrawRay(hitInfo.Point, hitInfo.sidewaysDir, Color.magenta);
				}
			}

		}

		

		

		private void SuspensionUpdate()
		{
			preOverflow = overflow;
			overflow = 0f;
			if (IsGrounded && Vector3.Dot(hitInfo.Normal, transform.up) > 0.1f)
			{
				bottomedOut = overExtended = false;

				length = transform.position.y - hitInfo.Point.y;

				if (length < 0f)
				{
					overflow = -length;
					length = 0f;
					bottomedOut = true;
				}
				else if (length > springInfo.SuspensionLength)
				{
					isGrounded = false;
					length = springInfo.SuspensionLength;
					overExtended = true;
				}
			}
			else
			{
				length = Mathf.Lerp(length, springInfo.SuspensionLength, Time.fixedDeltaTime * 8f);
			}

			springAndCenterDistance = length - wheelRadius;
			velocity = (length - preLength) / Time.fixedDeltaTime;
			compressionLength = springInfo.SuspensionLength - length;
			force = springInfo.SpringStrength * compressionLength;

			overflowVelocity = 0f;
			if (overflow > 0)
			{
				overflowVelocity = (overflow - preOverflow) / Time.fixedDeltaTime;
				bottomedOutForce = rig.mass * -Physics.gravity.y * Mathf.Clamp(overflowVelocity, 0f, Mathf.Infinity) * 0.0225f;
				rig.AddForceAtPosition(bottomedOutForce * transform.up, transform.position, ForceMode.Impulse);
			}
			else
			{
				damperInfo.MaxForce = length < preLength ? damperInfo.UnitBumpForce : damperInfo.UnitReboundForce;
				if (length <= preLength)
                {
					damperInfo.FinalForce = damperInfo.UnitBumpForce * 4f * velocity;
				}				
				else
                {
					damperInfo.FinalForce = -damperInfo.UnitReboundForce * 4f * velocity;
				}
					
			}

			preLength = length;
		}

		
		private void ForceUpdate()
		{
			if (!IsGrounded)
			{
				return;
			}

			float suspensionForceMagnitude = Mathf.Clamp(force + damperInfo.FinalForce, 0.0f, Mathf.Infinity);

			totalForce.x = suspensionForceMagnitude * hitInfo.Normal.x;
			totalForce.y = suspensionForceMagnitude * hitInfo.Normal.y;
			totalForce.z = suspensionForceMagnitude * hitInfo.Normal.z;

			rig.AddForceAtPosition(totalForce, hitInfo.Point);
		}

        private void OnDrawGizmos()
        {
			Gizmos.DrawSphere(transform.position, 0.1f);
			Handles.color = Color.white;
			if (isGrounded)
            {
				Gizmos.color = Color.white;
				Gizmos.DrawWireSphere(transform.position - transform.up * (springAndCenterDistance), wheelRadius);
				Gizmos.DrawSphere(hitInfo.Point, 0.08f);
				Handles.DrawLine(transform.position, transform.position - (transform.up * springAndCenterDistance), 1.6f);
			}
			else
            {
				Gizmos.color = Color.red;
				Gizmos.DrawWireSphere(transform.position - transform.up * (springInfo.SuspensionLength - wheelRadius), wheelRadius);
				Handles.DrawLine(transform.position, transform.position - (transform.up * (springInfo.SuspensionLength - wheelRadius)), 1.6f);
			}
			
            
        }
    }
}

