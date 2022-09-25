using Frontend.Scripts.Enums;
using Frontend.Scripts.Interfaces;
using Frontend.Scripts.Models;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Frontend.Scripts.Components
{
    public class UTWheel : MonoBehaviour
    {
        [Header("Settings")]
        [Range(0.1f, 2f)]
        [SerializeField] private float suspensionTravel = 1f;
        [SerializeField] private float wheelRadius = 0.35f;
        [SerializeField] private float tireMass = 20f;
        [Range(0, 1f)]
        [SerializeField] private float forwardTireGripFactor = 1f, sidewaysTireGripFactor = 1f;

        [SerializeField] private float spring = 20000f;
        [SerializeField] private float damper = 2000f;
        [SerializeField] private LayerMask layerMask;


        [SerializeField]
        private UTWheelDebug debugSettings = new UTWheelDebug()
        {
            DrawGizmos = true,
            DrawOnDisable = false,
            DrawMode = UTDebugMode.All,
            DrawForce = true,
            DrawWheelDirection = true,
            DrawSphereGizmo = true,
            DrawSprings = true,
        };

        private Rigidbody rig;
        private HitInfo hitInfo = new HitInfo();
        private bool isGrounded = false;
        private Vector3 tireWorldPosition;

        #region Telemetry/readonly

        private float previousSuspensionDistance = 0f;
        private float normalForce = 0f;
        private float compression = 0f;
        private float compressionRate = 0f;
        private float steerAngle = 0f;
        private float wheelAngle = 0f;
        private Vector3 suspensionForce;
        private Vector3 tirePosition;
        #endregion

        public bool IsGrounded => isGrounded;
        public HitInfo HitInfo => hitInfo;
        public float TireMass => tireMass;
        public float ForwardTireGripFactor => forwardTireGripFactor;
        public float SidewaysTireGripFactor => sidewaysTireGripFactor;
        public Vector3 TireWorldPosition => tireWorldPosition;
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
            tireWorldPosition = newPosition;

            Vector3 newVelocity = (newPosition - tirePosition) / Time.fixedDeltaTime;
            tirePosition = newPosition;

            normalForce = GetSuspensionForce(tirePosition) + tireMass * Mathf.Abs(Physics.gravity.y);
            suspensionForce = normalForce * transform.up;

            if (!isGrounded)
            {
                return;
            }

            rig.AddForceAtPosition(suspensionForce, tirePosition);
        }



        private Vector3 GetTirePosition()
        {
            isGrounded = Physics.SphereCast(transform.position, wheelRadius, -transform.up, out hitInfo.rayHit,  suspensionTravel, layerMask);
            if (isGrounded)
            {
                compression = hitInfo.Distance;
            }
            else
            {
                if(Physics.CheckSphere(transform.position, wheelRadius, layerMask))
                {
                    isGrounded = true;
                    hitInfo.rayHit = new RaycastHit()
                    {
                        point = transform.position - transform.up * wheelRadius,
                        normal = transform.up,
                        distance = 0,
                    };
                    compression = 0;
                }
                else
                {
                    compression = suspensionTravel;
                }
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


        #region DEBUG
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

            bool drawCurrently = (debugSettings.DrawGizmos) && (debugSettings.DrawMode == UTDebugMode.All)
                || (debugSettings.DrawMode == UTDebugMode.EditorOnly && !Application.isPlaying)
                || (debugSettings.DrawMode == UTDebugMode.PlaymodeOnly && Application.isPlaying);

            if (drawCurrently && (this.enabled) || (debugSettings.DrawOnDisable && !this.enabled))
            {
                if (rig != null)
                {
                    if(!Application.isPlaying)
                    {
                        tireWorldPosition = GetTirePosition();
                    }

                    if(debugSettings.DrawSprings)
                    {
                        Handles.DrawDottedLine(transform.position, tireWorldPosition, 1.1f);
                        Gizmos.color = Color.white;
                        Gizmos.DrawSphere(transform.position, .08f);
                        Gizmos.DrawSphere(tireWorldPosition, .08f);
                    }
                    
                    if (isGrounded)
                    {
                        Gizmos.color = Color.blue;
                        Gizmos.DrawSphere(hitInfo.Point, .08f);
                    }
                    if (debugSettings.DrawForce)
                    {
                        var force = (suspensionForce - normalForce * transform.up) / 1000f;
                        Gizmos.color = Color.yellow;
                        Gizmos.DrawLine(transform.position, transform.position + force);
                    }

                    if(debugSettings.DrawWheelDirection)
                    {
                        Handles.color = isGrounded ? Color.green : Color.red;
                        Handles.DrawLine(tireWorldPosition, tireWorldPosition + transform.forward, 2f);
                    }

                    

                    if(debugSettings.DrawSphereGizmo)
                    {
                        Gizmos.color = isGrounded ? Color.green : Color.red;
                        Gizmos.DrawWireSphere(tireWorldPosition, wheelRadius);
                    }
                }
            }
        }

        #endregion
    }
}
