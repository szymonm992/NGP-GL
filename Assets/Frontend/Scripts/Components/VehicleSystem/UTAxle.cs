using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Zenject;
using Frontend.Scripts.Enums;
using Frontend.Scripts.Interfaces;
using Frontend.Scripts.Models;
using GLShared.General.Enums;
using GLShared.General.Interfaces;
using GLShared.General.Models;

namespace Frontend.Scripts.Components
{
    public class UTAxle : MonoBehaviour, IInitializable, IVehicleAxle
    {
        public const float SUSPENSION_VISUALS_MOVEMENT_SPEED = 50F;

        [Inject(Id = "mainRig")] private readonly Rigidbody rig;
        [Inject] private readonly IVehicleController controller;
        [Inject] private readonly IPlayerInputProvider inputProvider;
        [Inject] private readonly IWheelReposition wheelReposition;

        [SerializeField] private UTAxlePair[] wheelPairs;
        [SerializeField] private bool canDrive;
        [SerializeField] private bool canSteer;
        [SerializeField] private bool invertSteer = false;
        

        [Header("Antiroll")]
        [SerializeField] private bool applyAntiroll;
        [SerializeField] private float antiRollForce = 0f;

        [Header("Optional")]
        [SerializeField] private float tiresContactOffset = 0f;
        [SerializeField] private bool repositionVisuals = true;

        private IPhysicsWheel leftAntirolled, rightAntirolled;
        private IEnumerable<IPhysicsWheel> allWheels;
        private IEnumerable<IPhysicsWheel> groundedWheels;
        private bool hasAnyWheel = false;

        public UTAxleDebug debugSettings = new UTAxleDebug()
        {
            DrawGizmos = true,
            DrawAxleCenter = true,
            DrawAxlePipes = true,
            DrawMode = UTDebugMode.All
        };

        public IEnumerable<UTAxlePair> WheelPairs => wheelPairs;
        public IEnumerable<IPhysicsWheel> AllWheels => allWheels;
        public IEnumerable<IPhysicsWheel> GroundedWheels => groundedWheels;

        public bool CanDrive => canDrive;
        public bool CanSteer => canSteer;
        public bool InvertSteer => invertSteer;
        public bool HasAnyWheelPair => wheelPairs.Any();
        public bool HasAnyWheel => hasAnyWheel;

        private void Awake()
        {
            allWheels = wheelPairs.Select(pair => pair.Wheel).ToArray();
            hasAnyWheel = allWheels.Any();
        }

        public void Initialize()
        {
            if (HasAnyWheelPair)
            {
                foreach(var pair in wheelPairs)
                {
                    pair.Initialize();
                }
            }

            groundedWheels = GetGroundedWheels();
            leftAntirolled = GetAllWheelsOfAxis(DriveAxisSite.Left).First();
            rightAntirolled = GetAllWheelsOfAxis(DriveAxisSite.Right).First();
        }

        private IEnumerable<IPhysicsWheel> GetGroundedWheels()
        {
            return allWheels.Where(wheel => wheel.IsGrounded == true);
        } 

        public IEnumerable<IPhysicsWheel> GetAllWheelsOfAxis(DriveAxisSite axis)
        {
            return wheelPairs.Where(pair => pair.Axis == axis).Select(pair => pair.Wheel).ToArray();
        }

        public void SetSteerAngle(float angleLeftAxis, float angleRightAxis)
        {
            foreach(var pair in wheelPairs)
            {
                pair.Wheel.SteerAngle = pair.Axis == DriveAxisSite.Left ? angleLeftAxis : angleRightAxis;
            }
        }

        private void FixedUpdate()
        {
            if (!wheelPairs.Any() || controller == null || controller.IsUpsideDown)
            {
                return;
            }

            groundedWheels = GetGroundedWheels();

            if (applyAntiroll)
            {
                CalculateAndApplyAntiroll();
            }

            if (repositionVisuals)
            {
                foreach (var pair in wheelPairs)
                {
                    if (pair.TireModel != null)
                    {
                        RepositionTireModel(pair);
                    }
                }
            }
        }

        private void CalculateAndApplyAntiroll()
        {
            float antiRollFinalForce = (leftAntirolled.CompressionRate - rightAntirolled.CompressionRate) * antiRollForce;
            if (leftAntirolled.IsGrounded)
            {
                rig.AddForceAtPosition(leftAntirolled.Transform.up * antiRollFinalForce,
                          leftAntirolled.UpperConstraintPoint);
            }
            
            if(rightAntirolled.IsGrounded)
            {
                rig.AddForceAtPosition(rightAntirolled.Transform.up * -antiRollFinalForce,
                 rightAntirolled.UpperConstraintPoint);
            }
        }

        private void RepositionTireModel(UTAxlePair pair)
        {
            var tireTransform = pair.VisualPartOfTire;

            if(controller.CurrentSpeed != 0)
            {
                float dir = -inputProvider.LastVerticalInput;
                Vector3 rotateAroundAxis = -tireTransform.right;
                wheelReposition.RotateWheels(dir, rotateAroundAxis, tireTransform, pair);
            }
            Vector3 tireWorldPos = pair.Wheel.CompressionRate < 1 ? pair.Wheel.TireWorldPosition : pair.Wheel.UpperConstraintPoint;
            Vector3 tireDesiredPosition = tireWorldPos + (pair.Wheel.Transform.up * tiresContactOffset);
            float movementSpeed = (controller.VisualElementsMovementSpeed * Mathf.Max(0.4f, controller.CurrentSpeedRatio)) * Time.deltaTime;
            tireTransform.position = Vector3.Lerp(tireTransform.position, tireDesiredPosition, movementSpeed);
            
            if(canSteer)
            {
                tireTransform.localRotation = Quaternion.Euler(tireTransform.localRotation.eulerAngles.x, pair.Wheel.SteerAngle, tireTransform.localRotation.eulerAngles.z);
            }

            wheelReposition.TrackMovement(tireTransform, pair, tireDesiredPosition, movementSpeed);
        }

        private void OnDrawGizmos()
        {
            #if UNITY_EDITOR
            bool drawCurrently = (debugSettings.DrawGizmos) && (debugSettings.DrawMode == UTDebugMode.All)
               || (debugSettings.DrawMode == UTDebugMode.EditorOnly && !Application.isPlaying)
               || (debugSettings.DrawMode == UTDebugMode.PlaymodeOnly && Application.isPlaying);

            if (drawCurrently)
            {
                Gizmos.color = Color.white;
                if(debugSettings.DrawAxleCenter)
                {
                    Gizmos.DrawSphere(transform.position, .11f);
                }
                
                if(debugSettings.DrawAxlePipes)
                {
                    if(!wheelPairs.Any())
                    {
                        return;
                    }

                    foreach (var pair in wheelPairs)
                    {
                        Handles.color = Color.white;
                        Handles.DrawLine(pair.Wheel.Transform.position, transform.position, 1.2f);
                    }
                }
                
            }
            #endif
        }
    }
}
