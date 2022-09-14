using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Frontend.Scripts.Components
{
	public class HoverSpring : MonoBehaviour
	{
		public Spring spring;
		public Damper damper;
		public Hit hit;
		public GameObject parent;
		public Rigidbody parentRigidbody;
		public bool grounded = false;
		public bool canDrive=true;

		public float driveForce = 0;



		public float inputX, inputY;
		public float absoluteInputY, absoluteInputX;
		private Vector3 wheelVelocityLocal;
		private float Fx, Fy;
		public bool isBrake;
		public float currentLongitudalGrip;



		public bool isLeft = false;
		public bool canSteer = false;
		public float steerAngle = 0f;
		private float wheelAngle = 0f;

		private void Awake()
		{
			if (parent == null) parent = FindParent();
			if (parent != null) parentRigidbody = parent.GetComponent<Rigidbody>();
		}

		void FixedUpdate()
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
			float rayLength = spring.maxLength;
			grounded = Physics.Raycast(transform.position, -transform.up * rayLength, out hit.rayHit, rayLength);
			Debug.DrawRay(transform.position, -transform.up * rayLength, Color.yellow);
			if (grounded)
			{
				hit.forwardDir = Vector3.Normalize(Vector3.Cross(hit.normal, -transform.right));
				hit.sidewaysDir = Quaternion.AngleAxis(90f, hit.normal) * hit.forwardDir;
				Debug.DrawRay(hit.point, hit.forwardDir, Color.blue);
				Debug.DrawRay(hit.point, hit.sidewaysDir, Color.magenta);
			}
		}

		private void ApplyFrictionForces()
		{
			Vector3 steeringDir = transform.right;
			Vector3 tireVel = parentRigidbody.GetPointVelocity(transform.position);

			float steeringVel = Vector3.Dot(steeringDir, tireVel);
			float desiredVelChange = -steeringVel * spring.tireGripFactor;
			float desiredAccel = desiredVelChange / Time.fixedDeltaTime;

			parentRigidbody.AddForceAtPosition(steeringDir * spring.tireMass * desiredAccel, hit.point);
		}

		private void Accelerate()
		{

				if (canDrive && !isBrake)
				{
					wheelVelocityLocal = transform.InverseTransformDirection(parentRigidbody.GetPointVelocity(hit.point));

					Fx = inputY * driveForce / 2;
					Fy = wheelVelocityLocal.x * driveForce;

					parentRigidbody.AddForceAtPosition((Fx * transform.forward) + (Fy * -transform.right), hit.point);
				}


		}

		private void Brakes()
		{
			currentLongitudalGrip = isBrake ? 1f : (absoluteInputY > 0 ? 0 : 0.5f);

					Vector3 forwardDir = transform.forward;
					Vector3 tireVel = parentRigidbody.GetPointVelocity(transform.position);

					float steeringVel = Vector3.Dot(forwardDir, tireVel);
					float desiredVelChange = -steeringVel * currentLongitudalGrip;
					float desiredAccel = desiredVelChange / Time.fixedDeltaTime;

					parentRigidbody.AddForceAtPosition(forwardDir * spring.tireMass * desiredAccel, transform.position);
		}

		private void SuspensionUpdate()
		{
			spring.preOverflow = spring.overflow;
			spring.overflow = 0f;
			if (grounded && Vector3.Dot(hit.normal, transform.up) > 0.1f)
			{
				spring.bottomedOut = spring.overExtended = false;

				spring.length = transform.position.y - hit.point.y;

				if (spring.length < 0f)
				{
					spring.overflow = -spring.length;
					spring.length = 0f;
					spring.bottomedOut = true;
				}
				else if (spring.length > spring.maxLength)
				{
					grounded = false;
					spring.length = spring.maxLength;
					spring.overExtended = true;
				}
			}
			else
			{
				spring.length = Mathf.Lerp(spring.length, spring.maxLength, Time.fixedDeltaTime * 8f);
			}

			spring.velocity = (spring.length - spring.preLength) / Time.fixedDeltaTime;
			spring.compressionLength = spring.maxLength - spring.length;
			spring.force = spring.strength * spring.compressionLength;

			spring.overflowVelocity = 0f;
			if (spring.overflow > 0)
			{
				spring.overflowVelocity = (spring.overflow - spring.preOverflow) / Time.fixedDeltaTime;
				spring.bottomedOutForce = parentRigidbody.mass * -Physics.gravity.y * Mathf.Clamp(spring.overflowVelocity, 0f, Mathf.Infinity) * 0.0225f;
				parentRigidbody.AddForceAtPosition(spring.bottomedOutForce * transform.up, transform.position, ForceMode.Impulse);
			}
			else
			{
				damper.maxForce = spring.length < spring.preLength ? damper.unitBumpForce : damper.unitReboundForce;
				if (spring.length <= spring.preLength)
					damper.force = damper.unitBumpForce * 4f * spring.velocity;
				else
					damper.force = -damper.unitReboundForce * 4f * spring.velocity;
			}

			spring.preLength = spring.length;
		}

		private Vector3 totalForce;
		private void ForceUpdate()
		{
			if (!grounded) return;

			float suspensionForceMagnitude = Mathf.Clamp(spring.force + damper.force, 0.0f, Mathf.Infinity);

			totalForce.x = suspensionForceMagnitude * hit.normal.x;
			totalForce.y = suspensionForceMagnitude * hit.normal.y;
			totalForce.z = suspensionForceMagnitude * hit.normal.z;

			parentRigidbody.AddForceAtPosition(totalForce, transform.position);

			Accelerate();
			Brakes();
			ApplyFrictionForces();

		}

		#region Class
		[System.Serializable]
		public class Hit
		{
			public RaycastHit rayHit;
			public int numCtp;
			public Vector3 forwardDir;
			public Vector3 sidewaysDir;

			public Vector3 point
			{
				get
				{
					return rayHit.point;
				}
			}
			public Vector3 normal
			{
				get
				{
					return rayHit.normal;
				}
			}
		}
		[System.Serializable]
		public class Spring
		{
			public float strength = 21000f;
			public float maxLength = 0.3f;
			[Range(0,1f)]
			public float tireGripFactor = 1f;
			public float tireMass = 40f;
			[HideInInspector] public float force;
			[HideInInspector] public float velocity;
			[HideInInspector] public float preLength, length;
			[HideInInspector] public float compressionLength;
			[HideInInspector] public float preOverflow, overflow, overflowVelocity;
			[HideInInspector] public bool bottomedOut, overExtended;
			[HideInInspector] public float bottomedOutForce;
		}
		[System.Serializable]
		public class Damper
		{
			public float unitBumpForce = 800.0f;
			public float unitReboundForce = 1000.0f;
			[HideInInspector] public float force;
			[HideInInspector] public float maxForce;
		}
		#endregion
		private GameObject FindParent()
		{
			Transform t = transform;
			while (t != null)
			{
				if (t.GetComponent<Rigidbody>())
				{
					return t.gameObject;
				}
				else
				{
					t = t.parent;
				}
			}
			return null;
		}
	}
}

