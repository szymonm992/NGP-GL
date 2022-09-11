using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Frontend.Scripts
{
    public class TestWheel : MonoBehaviour
    {
        [SerializeField] private float restLength;
        [SerializeField] private float springTravel;
        [SerializeField] private float springStiffness;
        [SerializeField] private float damperStiffness;
        [SerializeField] private float wheelRadius;
        [SerializeField] private float tireMass = 20f;
        [Range(0,1f)]
        [SerializeField] private float tireGripFactor = 1f;

        public bool isLeft = false;
        public bool canSteer = false;
        public bool canDrive = false;
        public float steerAngle = 0f;
        private float wheelAngle = 0f;

        private float minLength;
        private float maxLength;
        private float lastLength;
        private float springLength;
        private float springVelocity;
        private float springForce;
        private float damperForce;
        private float wheelCompression;

        private Vector3 suspensionForce;
        private Vector3 wheelWorldPosition;
        private Rigidbody rig;
        private bool isGrounded = false;
        private RaycastHit hit;
        private float debugLength;

        private Vector3 lastPosition;

        public float SpringForce => springForce;
        public float WheelCompression => wheelCompression;
        public bool IsGrounded => isGrounded;
        public RaycastHit Hit => hit;

        private void OnValidate()
        {
            RefreshParams();
        }

        private void Start()
        {
            rig = transform.root.GetComponent<Rigidbody>();
            OnValidate();
        }

        private void Update()
        {
            wheelAngle = Mathf.Lerp(wheelAngle, steerAngle, Time.deltaTime * 8f);
            transform.localRotation = Quaternion.Euler(transform.localRotation.x, 
                transform.localRotation.y + wheelAngle,
                transform.localRotation.z);
        }

        private void RefreshParams()
        {
            minLength = restLength - springTravel;
            maxLength = restLength + springTravel;
            springLength = minLength+maxLength/2;

            debugLength  = (springLength + wheelRadius);
            wheelWorldPosition = transform.position - transform.up * debugLength;

            wheelCompression = debugLength;
        }

        private void FixedUpdate()
        {
            isGrounded = Physics.SphereCast(transform.position,wheelRadius, -transform.up, out hit, maxLength + wheelRadius);
            if (isGrounded)
            {
                wheelCompression = hit.distance;
                ApplySpringForces();
                ApplyFrictionForces();
            }
            else
            {
                debugLength = (maxLength + wheelRadius);
                wheelCompression = debugLength;
                wheelWorldPosition = transform.position - transform.up * debugLength;
            }
        }   

        private void ApplySpringForces()
        {
            lastLength = springLength;
            springLength = hit.distance - wheelRadius;
            springLength = Mathf.Clamp(springLength, minLength, maxLength);
            springVelocity = (lastLength - springLength) / Time.fixedDeltaTime;
            springForce = springStiffness * (restLength - springLength);
            damperForce = damperStiffness * springVelocity;

            suspensionForce = (springForce + damperForce) * transform.up;

            rig.AddForceAtPosition(suspensionForce, hit.point);

            debugLength = (springLength + wheelRadius);
            wheelWorldPosition = hit.point + transform.up * wheelRadius;
        }

        private void ApplyFrictionForces()
        {
            Vector3 steeringDir = transform.right;
            Vector3 tireVel = rig.GetPointVelocity(transform.position);

            float steeringVel = Vector3.Dot(steeringDir, tireVel);
            float desiredVelChange = -steeringVel * tireGripFactor;
            float desiredAccel = desiredVelChange / Time.fixedDeltaTime;

            rig.AddForceAtPosition(steeringDir * tireMass * desiredAccel, hit.point);
        }

        private void OnDrawGizmos()
        {
            #if UNITY_EDITOR
            if(!Application.isPlaying && lastPosition != transform.position)
            {
                OnValidate();
            }
            #endif
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(transform.position, 0.04f);
            Gizmos.DrawSphere(wheelWorldPosition, 0.04f);

            Gizmos.color = isGrounded ? Color.green : Color.red;

            Gizmos.DrawWireSphere(wheelWorldPosition, wheelRadius);
         //   Gizmos.DrawLine(transform.position, transform.position - transform.up * debugLength);

            Handles.color = isGrounded ? Color.green : Color.red;
            Handles.DrawDottedLine(transform.position, transform.position - (transform.up * wheelCompression), 1.1f);

            Gizmos.DrawSphere(hit.point, 0.04f);
            #if UNITY_EDITOR
            lastPosition = transform.position;
            #endif
        }

        #region ALTERNATIVE DRIVE
        /*
        private void Driving(Transform wheel,float input)
        {
            Vector3 accelDir = wheel.forward;
            if(Mathf.Abs(input)>0f)
            {
                float carSpeed = Vector3.Dot(transform.forward, rig.velocity);

                float normalizedSpeed = Mathf.Clamp01(Mathf.Abs(carSpeed) / carTopSpeed);

                float availableTorque = powerCurve.Evaluate(normalizedSpeed) * input;
                rig.AddForceAtPosition(accelDir * availableTorque, wheel.position);
            }
        }*/
        #endregion


    }
}
