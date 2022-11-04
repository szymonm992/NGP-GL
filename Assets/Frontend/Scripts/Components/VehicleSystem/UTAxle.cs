using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Zenject;
using Frontend.Scripts.Enums;
using Frontend.Scripts.Interfaces;
using Frontend.Scripts.Models;
using GLShared.General.Enums;

namespace Frontend.Scripts.Components
{
    public class UTAxle : MonoBehaviour, IInitializable
    {
        [Inject(Id = "mainRig")] private readonly Rigidbody rig;
        [Inject] private readonly IVehicleController controller;
        [Inject] private readonly IPlayerInputProvider inputProvider;
        [Inject] private readonly IWheelReposition wheelReposition;

        [SerializeField] private UTAxlePair[] wheelPairs;
        [SerializeField] private bool canDrive;
        [SerializeField] private bool canSteer;
        [SerializeField] private bool repositionVisuals = true;

        [Header("Antiroll")]
        [SerializeField] private bool applyAntiroll;
        [SerializeField] private float antiRollForce = 0f;

        private UTWheel leftAntirolled, rightAntirolled;

        public UTAxleDebug debugSettings = new UTAxleDebug()
        {
            DrawGizmos = true,
            DrawAxleCenter = true,
            DrawAxlePipes = true,
            DrawMode = UTDebugMode.All
        };

        public IEnumerable<UTAxlePair> WheelPairs => wheelPairs;
        public bool CanDrive => canDrive;
        public bool CanSteer => canSteer;
        public bool HasAnyWheelPair => wheelPairs.Any();

        public void Initialize()
        {
            if(HasAnyWheelPair)
            {
                foreach(var pair in wheelPairs)
                {
                    pair.Initialize();
                }
            }
            leftAntirolled = GetAllWheelsOfAxis(DriveAxisSite.Left).First();
            rightAntirolled = GetAllWheelsOfAxis(DriveAxisSite.Right).First();
        }

        public IEnumerable<UTWheel> GetGroundedWheels()
        {
            return wheelPairs.Where(pair => pair.Wheel.IsGrounded == true).Select(pair => pair.Wheel).ToArray();
        } 

        public IEnumerable<UTWheel> GetAllWheels()
        {
            return wheelPairs.Select(pair => pair.Wheel).ToArray();
        }
        public IEnumerable<UTWheel> GetAllWheelsOfAxis(DriveAxisSite axis)
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

        private void Update()
        {
            if (!wheelPairs.Any() || controller == null || controller.IsUpsideDown)
            {
                return;
            }

            if(repositionVisuals)
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
        private void FixedUpdate()
        {
            if (!wheelPairs.Any() || controller.IsUpsideDown)
            {
                return;
            }

            if (applyAntiroll)
            {
                CalculateAndApplyAntiroll();
            }
        }

        private void CalculateAndApplyAntiroll()
        {
            float antiRollFinalForce = (leftAntirolled.CompressionRate - rightAntirolled.CompressionRate) * antiRollForce;
            if (leftAntirolled.IsGrounded)
            {
                rig.AddForceAtPosition(leftAntirolled.transform.up * antiRollFinalForce,
                          leftAntirolled.UpperConstraintPoint);
            }
            
            if(rightAntirolled.IsGrounded)
            {
                rig.AddForceAtPosition(rightAntirolled.transform.up * -antiRollFinalForce,
                 rightAntirolled.UpperConstraintPoint);
            }
        }

        private void RepositionTireModel(UTAxlePair pair)
        {
            var tireTransform = pair.VisualPartOfTire;

            float dir = -inputProvider.LastVerticalInput;
            Vector3 rotateAroundAxis = -tireTransform.right;

            wheelReposition.RotateWheels(dir, rotateAroundAxis, tireTransform, pair,  out float currentToMaxRatio);
            
            Vector3 tireDesiredPosition = pair.Wheel.TireWorldPosition;
            tireTransform.position = Vector3.Lerp(tireTransform.position, tireDesiredPosition, (75f * Mathf.Max(0.5f, currentToMaxRatio)) * Time.deltaTime);
            
            if(canSteer)
            {
                tireTransform.localRotation = Quaternion.Euler(tireTransform.localRotation.eulerAngles.x, pair.Wheel.SteerAngle, tireTransform.localRotation.eulerAngles.z);
            }

            wheelReposition.TrackMovement(tireTransform, pair, tireDesiredPosition);
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
                        Handles.DrawLine(pair.Wheel.transform.position, transform.position, 1.2f);
                    }
                }
                
            }
            #endif
        }
    }
}
