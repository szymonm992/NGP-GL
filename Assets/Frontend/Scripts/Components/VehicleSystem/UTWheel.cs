using Frontend.Scripts.Enums;
using Frontend.Scripts.Interfaces;
using Frontend.Scripts.Models;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UInc.Core.Utilities;
using UnityEditor;
using UnityEngine;
using Zenject;

namespace Frontend.Scripts.Components
{
    public class UTWheel : MonoBehaviour
    {
        [Inject] private Rigidbody rig;

        [Header("Settings")]
        [Range(0.1f, 2f)]
        [SerializeField] private float suspensionTravel = 1f;
        [SerializeField] private float wheelRadius = 0.35f;
        [SerializeField] private float tireMass = 20f;
        [Range(0, 1f)]
        [SerializeField] private float forwardTireGripFactor = 1f, sidewaysTireGripFactor = 1f;

        [SerializeField] private float spring = 20000f;
        [SerializeField] private float damper = 2000f;
        [Range(-1f, 0)]
        [SerializeField] private float hardPointOfTire = -.2f;
        [SerializeField] private LayerMask layerMask;
        [SerializeField] private Rigidbody localRig;
        [SerializeField] private MeshCollider myCollider;

        [SerializeField]
        private UTWheelDebug debugSettings = new UTWheelDebug()
        {
            DrawGizmos = true,
            DrawOnDisable = false,
            DrawMode = UTDebugMode.All,
            DrawForce = true,
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
        public float compressionRate = 0f;
        private float steerAngle = 0f;
        private float wheelAngle = 0f;
        private float absGravity;
        private Vector3 suspensionForce;
        private Vector3 tirePosition;


        private float finalTravelLength;
        private float hardPointAbs;
        private Vector3 lowestSpringPosition, highestSpringPosition;
        
     
        #endregion

        public bool IsGrounded => isGrounded;
        public HitInfo HitInfo => hitInfo;
        public float TireMass => tireMass;
        public float ForwardTireGripFactor => forwardTireGripFactor;
        public float SidewaysTireGripFactor => sidewaysTireGripFactor;
        public float CompressionRate => compressionRate;
        public float HardPointAbs => hardPointAbs;
        public Vector3 TireWorldPosition => tirePosition; 
        public Vector3 HighestSpringPosition => highestSpringPosition; 
        
        public float SteerAngle
        {
            get => wheelAngle;
            set => this.steerAngle = value;
        }

        private void Awake()
        {
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
        }

        private void SetIgnoredColliders()
        {
            var allColliders = transform.root.GetComponentsInChildren<Collider>();

            if (allColliders.Any())
            {
                allColliders.ForEach(collider => Physics.IgnoreCollision(myCollider, collider, true));
            }
        }

        private void Update()
        {
            if(wheelAngle != steerAngle)
            {
                wheelAngle = Mathf.Lerp(wheelAngle, steerAngle, Time.deltaTime * 8f);
                transform.localRotation = Quaternion.Euler(transform.localRotation.x,
                    transform.localRotation.y + wheelAngle,
                    transform.localRotation.z);
            }
        }

        private void FixedUpdate()
        {
         
            Vector3 newPosition = GetTirePosition();

            if (compressionRate == 1)
            {
                AntigravityHelper();
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


        private void AntigravityHelper()
        {
            if (rig.velocity.y < -4f)
            {
                rig.AddForce(Vector3.up * Mathf.Min(-rig.velocity.y, 7f), ForceMode.VelocityChange);
            }
        }

        private Vector3 GetTirePosition()
        {
           
            lowestSpringPosition = transform.position - transform.up * finalTravelLength;
            highestSpringPosition = transform.position + transform.up * hardPointOfTire;


            isGrounded = (localRig.SweepTest(-transform.up, out hitInfo.rayHit, finalTravelLength));
            Vector3 tirePos = transform.position - (transform.up * finalTravelLength);

            if (isGrounded)
            {
                tirePos = transform.position - (transform.up * hitInfo.Distance);
                extension = Vector3.Distance(highestSpringPosition, tirePos) / suspensionTravel;
                if ((highestSpringPosition - transform.position).sqrMagnitude < (tirePosition - transform.position).sqrMagnitude)
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

            /*
            isGrounded = (Physics.CheckSphere(transform.position, wheelRadius, layerMask));

            if (isGrounded)
            {
                hitInfo.rayHit = new RaycastHit()
                {
                    point = transform.position - transform.up * wheelRadius,
                    normal = transform.up,
                    distance = 0,
                };
                extension = 0;
                compressionRate = 1;

                if(rig.velocity.y < -4f)
                {
                    rig.AddForce(Vector3.up * Mathf.Min(-rig.velocity.y, 7f), ForceMode.VelocityChange);
                } 
            }
            else
            {
                if (Physics.SphereCast(transform.position, wheelRadius, -transform.up, out hitInfo.rayHit, suspensionTravel, layerMask))
                {
                    isGrounded = true;
                    extension = hitInfo.Distance;
                    compressionRate = 1 - (hitInfo.Distance / suspensionTravel);
                }
                else
                {
                    extension = suspensionTravel;
                    compressionRate = 0;
                }
            }
            return transform.position - (transform.up * extension);*/
        }

        private float GetSuspensionForce(Vector3 tirePosition)
        {
            float distance = Vector3.Distance(transform.position - transform.up * finalTravelLength, tirePosition);
            float springForce = spring * distance;
            float damperForce = damper * ((distance - previousSuspensionDistance) / Time.fixedDeltaTime);
            previousSuspensionDistance = distance;
            return springForce + damperForce;
        }

        #if UNITY_EDITOR
        #region DEBUG
        private void OnValidate()
        {
            if (!rig)
            {
                rig = transform.GetComponentInParent<Rigidbody>();
               
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
                    if(!Application.isPlaying)
                    {
                        tirePosition = GetTirePosition();
                    }

                    if(debugSettings.DrawSprings)
                    {
                        Handles.DrawDottedLine(transform.position, tirePosition, 1.1f);
                        Gizmos.color = Color.red;
                        Gizmos.DrawSphere(highestSpringPosition, .08f);
                        Gizmos.color = Color.blue;
                        Gizmos.DrawSphere(lowestSpringPosition, .08f);
                        Gizmos.color = Color.white;
                        Gizmos.DrawSphere(tirePosition, .08f);
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
                        Handles.DrawLine(tirePosition, tirePosition + transform.forward, 2f);
                    }

                    

                    if(debugSettings.DrawShapeGizmo)
                    {
                        Gizmos.color = isGrounded ? Color.green : Color.red;
                        //Gizmos.DrawWireSphere(tirePosition, wheelRadius);

                        Gizmos.DrawWireMesh(myCollider.sharedMesh, tirePosition, transform.rotation, transform.lossyScale);
                    }
                }
            }
        }

        #endregion
        #endif
    }
}
