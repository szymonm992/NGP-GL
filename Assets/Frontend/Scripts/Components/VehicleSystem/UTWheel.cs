using Frontend.Scripts.Enums;
using Frontend.Scripts.Interfaces;
using Frontend.Scripts.Models;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Frontend.Scripts.Components
{
    public class UTWheel : MonoBehaviour, ICustomWheel
    {
        private Rigidbody rig;

        [SerializeField]
        private UnderWheelDebug debugSettings = new UnderWheelDebug()
        {
            DrawGizmos = true,
            DrawOnDisable = false,
            DrawMode = UnderWheelDebugMode.All,
            DrawForce = true
        };

        [Header("Settings")]
        [SerializeField] [Range(0.1f, 1f)] private float suspensionTravel = 1f;
        [SerializeField] private float wheelRadius = 0.35f;
        [SerializeField] private float tireMass = 20f;
        [Range(0, 2f)]
        [SerializeField] private float forwardTireGripFactor = 1f, sidewaysTireGripFactor = 1f;

        [SerializeField] private float spring = 20000f;
        [SerializeField] private float damper = 2000f;
        [SerializeField] private float rollingResistance = 0.01f;
        [SerializeField] private float inertia = 3f;
        [SerializeField] private bool canDrive = true;
        [SerializeField] private bool canSteer = true;
        [SerializeField] private LayerMask layerMask;
        [SerializeField] private DriveAxisSite wheelAxis;

        private HitInfo hitInfo = new HitInfo();
        private float motorTorque = 0f;
        private float brakeTorque = 0f;
        private float steerAngle = 0f;
        private float wheelAngle = 0f;

        private bool isGrounded = false;


        #region Telemetry/readonly
        private float angularVelocity = 0f;
        private float tireRotation = 0f;

        private float differentialSlipRatio = 0f;
        private float differentialTanOfSlipAngle = 0f;
        private float slipAngle = 0f;
        private float previousSuspensionDistance = 0f;
        private float normalForce = 0f;
        private float compression = 0f;

        private Vector3 finalForce;
        private Vector3 tirePosition;

        private const float RelaxationLengthLongitudal = 0.103f;
        private const float RelaxationLengthLateral = 0.303f;
        #endregion

        public bool IsGrounded => isGrounded;
        public bool CanDrive => canDrive;
        public HitInfo HitInfo => hitInfo;
        public float TireMass => tireMass;
        public float ForwardTireGripFactor => forwardTireGripFactor;
        public float SidewaysTireGripFactor => sidewaysTireGripFactor;
        public bool CanSteer => canSteer;
        public DriveAxisSite WheelAxis => wheelAxis;
        public float SteerAngle
        {
            get => steerAngle;
            set => this.steerAngle = value;
        }

        private void Update()
        {
            wheelAngle = Mathf.Lerp(wheelAngle, steerAngle, Time.deltaTime * 8f);
            transform.localRotation = Quaternion.Euler(transform.localRotation.x,
                transform.localRotation.y + wheelAngle,
                transform.localRotation.z);
        }

        private void FixedUpdate()
        {
            Vector3 newPosition = GetTirePosition();
            Vector3 newVelocity = (newPosition - tirePosition) / Time.fixedDeltaTime;
            tirePosition = newPosition;
            normalForce = GetSuspensionForce(tirePosition) + tireMass * Mathf.Abs(Physics.gravity.y);

            //float longitudalSpeed = Vector3.Dot(newVelocity, transform.forward);
            //float lateralSpeed = Vector3.Dot(newVelocity, -transform.right);

            //float lateralForce = GetLateralForce(normalForce, lateralSpeed, longitudalSpeed);
            //float longitudalForce = GetLongitudalForce(normalForce, longitudalSpeed);
            finalForce = normalForce * transform.up;

            if (!isGrounded)
            {
                return;
            }

            rig.AddForceAtPosition(finalForce, tirePosition);

           // UpdateAngularVelocity(longitudalForce);
        }

        public void GetWorldPosition(out Vector3 position, out Quaternion rotation)
        {
            var localTireRotation = Quaternion.Euler(tireRotation * Mathf.Rad2Deg, 0f, 0f);
            position = GetTirePosition();
            rotation = transform.rotation * localTireRotation;
        }

        public Vector3 GetTirePosition()
        {
            isGrounded = Physics.SphereCast(transform.position, wheelRadius, -transform.up, out hitInfo.rayHit,  suspensionTravel, layerMask);
            if (isGrounded)
            {
                compression = hitInfo.Distance;
            }
            else
            {
                compression = suspensionTravel;
            }
            return transform.position - (transform.up * compression);
        }

        private float GetSuspensionForce(Vector3 localTirePosition)
        {
            float distance = Vector3.Distance(transform.position - transform.up * suspensionTravel, localTirePosition);
            float springForce = spring * distance;
            float damperForce = damper * ((distance - previousSuspensionDistance) / Time.fixedDeltaTime);
            previousSuspensionDistance = distance;
            return springForce + damperForce;
        }

        private void OnValidate()
        {
            if (!rig)
            {
                rig = transform.GetComponentInParent<Rigidbody>();
            }
            if (compression == 0f)
            {
                compression = suspensionTravel;
            }

        }
        private void OnDrawGizmos()
        {

            bool drawCurrently = (debugSettings.DrawGizmos) && (debugSettings.DrawMode == UnderWheelDebugMode.All)
                || (debugSettings.DrawMode == UnderWheelDebugMode.EditorOnly && !Application.isPlaying)
                || (debugSettings.DrawMode == UnderWheelDebugMode.PlaymodeOnly && Application.isPlaying);

            if (drawCurrently && (debugSettings.DrawOnDisable && !this.enabled) || (this.enabled))
            {
                if (rig != null)
                {
                    Handles.color = isGrounded ? Color.green : Color.red;

                    GetWorldPosition(out Vector3 position, out Quaternion rotation);

                    Gizmos.DrawWireSphere(position, wheelRadius);
                    Handles.DrawDottedLine(transform.position, position, 1.1f);

                    if(isGrounded)
                    {
                        Gizmos.DrawSphere(hitInfo.Point, .08f);
                    }

                    if (debugSettings.DrawForce)
                    {
                        var force = (finalForce - normalForce * transform.up) / 1000f;
                        Gizmos.color = Color.blue;
                        Gizmos.DrawLine(transform.position, transform.position + force);
                    }

                    Gizmos.color = isGrounded ? Color.green : Color.red;
                    Gizmos.DrawSphere(transform.position, .08f);
                    Gizmos.DrawSphere(position, .08f);
                }
            }
        }
    }
}
