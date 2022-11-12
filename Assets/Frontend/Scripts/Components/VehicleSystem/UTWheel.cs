using System.Linq;
using UInc.Core.Utilities;
using UnityEditor;
using UnityEngine;
using Zenject;
using Frontend.Scripts.Enums;
using Frontend.Scripts.Models;
using GLShared.General.ScriptableObjects;
using GLShared.General.Models;
using Frontend.Scripts.Interfaces;

namespace Frontend.Scripts.Components
{
    public class UTWheel : MonoBehaviour, IInitializable
    {
        private const float WHEEL_TURN_RATIO = 8f;

        [Inject] private readonly GameParameters gameParameters;
        [Inject(Id = "mainRig")] private Rigidbody rig;
        [Inject] private readonly IVehicleController vehicleController;

        [Header("Settings")]
        [Range(0.1f, 2f)]
        [SerializeField] private float suspensionTravel = 0.5f;
        [SerializeField] private float wheelRadius = 0.7f;
        [SerializeField] private float tireMass = 60f;
        [Range(0, 1f)]
        [SerializeField] private float forwardTireGripFactor = 1f, sidewaysTireGripFactor = 1f;

        [SerializeField] private float spring = 20000f;
        [SerializeField] private float damper = 3000f;
        [Range(-3f, 0)]
        [SerializeField] private float hardPointOfTire = -0.7f;
        [SerializeField] private Transform upperConstraintTransform;
        [SerializeField] private Transform lowerConstraintTransform;


        [SerializeField]
        private UTWheelDebug debugSettings = new UTWheelDebug()
        {
            DrawGizmos = true,
            DrawOnDisable = false,
            DrawMode = UTDebugMode.All,
            DrawWheelDirection = true,
            DrawShapeGizmo = true,
            DrawSprings = true,
        };

        #region Telemetry/readonly
        private HitInfo hitInfo = new HitInfo();
        private bool isGrounded = false;

        private float previousSuspensionDistance = 0f;
        private float normalForce = 0f;
        private float extension = 0f;
        private float compressionRate = 0f;
        private float steerAngle = 0f;
        private float wheelAngle = 0f;

        private float absGravity;
        private float finalTravelLength;
        private float hardPointAbs;

        private Vector3 suspensionForce;
        private Vector3 tirePosition;

        private Rigidbody localRig;
        private MeshCollider localCollider;

        #endregion

        public bool IsGrounded => isGrounded;
        public HitInfo HitInfo => hitInfo;
        public float WheelRadius => wheelRadius;
        public float TireMass => tireMass;
        public float ForwardTireGripFactor => forwardTireGripFactor;
        public float SidewaysTireGripFactor => sidewaysTireGripFactor;
        public float CompressionRate => compressionRate;
        public float HardPointAbs => hardPointAbs;
        public Vector3 TireWorldPosition => tirePosition;
        public Vector3 UpperConstraintPoint => upperConstraintTransform.position;
        public Vector3 LowerConstraintPoint => lowerConstraintTransform.position;

        public float SteerAngle
        {
            get => wheelAngle;
            set => this.steerAngle = value;
        }

        public void Initialize()
        {
            localRig = GetComponent<Rigidbody>();
            localCollider = GetComponent<MeshCollider>();

            AssignPrimaryParameters();
            SetIgnoredColliders();
        }

        private void AssignPrimaryParameters()
        {
            if (extension == 0f)
            {
                extension = suspensionTravel;
            }

            absGravity = Mathf.Abs(Physics.gravity.y);
            hardPointAbs = Mathf.Abs(hardPointOfTire);
            finalTravelLength = suspensionTravel + hardPointAbs;

            if (upperConstraintTransform != null)
            {
                Vector3 highestPoint = transform.position + transform.up * hardPointOfTire;
                upperConstraintTransform.position = highestPoint;
            }

            if (lowerConstraintTransform != null)
            {
                Vector3 lowestPoint = transform.position + transform.up * -finalTravelLength;
                lowerConstraintTransform.position = lowestPoint;
            }
        }

        private void SetIgnoredColliders()
        {
            var allColliders = transform.root.GetComponentsInChildren<Collider>();

            if (allColliders.Any() && localCollider)
            {
                allColliders.ForEach(collider => Physics.IgnoreCollision(localCollider, collider, true));
            }
        }

        private void Update()
        {
            if (wheelAngle != steerAngle)
            {
                wheelAngle = Mathf.Lerp(wheelAngle, steerAngle, Time.deltaTime * WHEEL_TURN_RATIO);
                transform.localRotation = Quaternion.Euler(transform.localRotation.x,
                    transform.localRotation.y + wheelAngle,
                    transform.localRotation.z);
            }
        }

        private void FixedUpdate()
        {
            Vector3 newPosition = GetTirePosition();

            if (compressionRate == 1 && vehicleController.DoesGravityDamping)
            {
                GravityDamping();
            }

            tirePosition = newPosition;

            normalForce = GetSuspensionForce(tirePosition) + tireMass * absGravity;
            suspensionForce = normalForce * transform.up;

            if (!isGrounded)
            {
                return;
            }

            rig.AddForceAtPosition(suspensionForce, tirePosition);
        }


        private void GravityDamping()
        {
            if (rig.velocity.y < -4f)
            {
                rig.AddForce(Vector3.up * Mathf.Min(-rig.velocity.y, 4f), ForceMode.VelocityChange);
            }
        }

        private Vector3 GetTirePosition()
        {
            if (localRig.SweepTest(-transform.up, out hitInfo.rayHit, finalTravelLength))
            {
                hitInfo.CalculateNormalAndUpDifferenceAngle();
                isGrounded = (hitInfo.NormalAndUpAngle <= gameParameters.MaxWheelDetectionAngle);
            }
            else
            {
                isGrounded = false;
            }

            Vector3 tirePos = transform.position - (transform.up * finalTravelLength);

            if (isGrounded)
            {
                tirePos = transform.position - (transform.up * hitInfo.Distance);
                extension = Vector3.Distance(upperConstraintTransform.position, tirePos) / suspensionTravel;
                if (hardPointAbs < (tirePosition - transform.position).sqrMagnitude)
                {
                    compressionRate = 1 - extension;
                }
                else
                {
                    compressionRate = 1;
                }
            }
            else
            {
                extension = 1;
                compressionRate = 0;
            }
            return tirePos;
        }

        private float GetSuspensionForce(Vector3 tirePosition)
        {
            float distance = Vector3.Distance(lowerConstraintTransform.position, tirePosition);
            float springForce = spring * distance;
            float damperForce = damper * ((distance - previousSuspensionDistance) / Time.fixedDeltaTime);
            previousSuspensionDistance = distance;
            return springForce + damperForce;
        }

        #if UNITY_EDITOR
        #region DEBUG
        private void OnValidate()
        {
            if (rig == null)
            {
                rig = transform.GetComponentInParent<Rigidbody>();
                localRig = GetComponent<Rigidbody>();
                localCollider = GetComponent<MeshCollider>();
            }

            AssignPrimaryParameters();
            SetIgnoredColliders();
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
                    if (!Application.isPlaying)
                    {
                        tirePosition = GetTirePosition();
                    }

                    if (debugSettings.DrawSprings)
                    {
                        Handles.DrawDottedLine(transform.position, tirePosition, 1.1f);


                        if (upperConstraintTransform != null)
                        {
                            Handles.color = Color.white;
                            //Gizmos.color = Color.yellow;
                            Handles.DrawLine(upperConstraintTransform.position + transform.forward * 0.1f, upperConstraintTransform.position - transform.forward * 0.1f, 2f);
                            //Gizmos.DrawSphere(highestPointTransform.position, .08f);
                        }

                        if (lowerConstraintTransform != null)
                        {
                            Handles.color = Color.white;
                            //Gizmos.color = Color.yellow;
                            Handles.DrawLine(lowerConstraintTransform.position + transform.forward * 0.1f, lowerConstraintTransform.position - transform.forward * 0.1f, 2f);

                            //Gizmos.DrawSphere(lowestPointTransform.position, .08f);
                        }

                        Handles.color = Color.white;
                        //Gizmos.DrawSphere(tirePosition, .08f);
                        Handles.DrawLine(tirePosition + transform.forward * 0.05f, tirePosition - transform.forward * 0.05f, 4f);

                    }

                    if (isGrounded)
                    {
                        Gizmos.color = Color.blue;
                        Gizmos.DrawSphere(hitInfo.Point, .08f);
                    }

                    if (debugSettings.DrawWheelDirection)
                    {
                        Handles.color = isGrounded ? Color.green : Color.red;
                        Handles.DrawLine(tirePosition, tirePosition + transform.forward, 2f);
                    }



                    if (debugSettings.DrawShapeGizmo)
                    {
                        Gizmos.color = isGrounded ? Color.green : Color.red;
                        //Gizmos.DrawWireSphere(tirePosition, wheelRadius);

                        Gizmos.DrawWireMesh(localCollider.sharedMesh, tirePosition, transform.rotation, transform.lossyScale);
                    }
                }
            }
        }

        #endregion
        #endif
    }
}
