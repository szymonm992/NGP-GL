using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Zenject;
using Frontend.Scripts.Interfaces;
using Frontend.Scripts.Models;
using GLShared.General.Enums;
using GLShared.General.Interfaces;
using GLShared.General.Models;

namespace Frontend.Scripts.Components
{
    public class UTAxle : UTAxleBase
    {
        [Header("Antiroll")]
        [SerializeField] protected bool applyAntiroll;
        [SerializeField] protected float antiRollForce = 0f;

        [Header("Optional")]
        [SerializeField] protected float tiresContactOffset = 0f;

        private IPhysicsWheel leftAntirolled, rightAntirolled;

        public override void Initialize()
        {
            base.Initialize();
            leftAntirolled = GetAllWheelsOfAxis(DriveAxisSite.Left).First();
            rightAntirolled = GetAllWheelsOfAxis(DriveAxisSite.Right).First();
        }

        public override void SetSteerAngle(float angleLeftAxis, float angleRightAxis)
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

            if (repositionVisuals && vehicleModelEffects.IsInsideCameraView)
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

        protected override void RepositionTireModel(UTAxlePair pair)
        {
            base.RepositionTireModel(pair);
            var tireTransform = pair.VisualPartOfTire;

            if (controller.CurrentSpeed != 0)
            {
                float dir = -inputProvider.LastVerticalInput;
                Vector3 rotateAroundAxis = -tireTransform.right;
                wheelReposition.RotateWheel(dir, rotateAroundAxis, tireTransform, pair);
            }
            
            bool isNotfullyCompressed = pair.Wheel.CompressionRate < 1f;
            Vector3 tireWorldPos = isNotfullyCompressed ? pair.Wheel.TireWorldPosition : pair.Wheel.UpperConstraintPoint;
            Vector3 tireDesiredPosition = tireWorldPos + (pair.Wheel.Transform.up * tiresContactOffset);

            float wheelRepositionSpeedMultiplier = isNotfullyCompressed ? wheelReposition.RepositionSpeed : 0.1f;
            float movementSpeed = (pair.Wheel.IsGrounded ? wheelRepositionSpeedMultiplier : 5f) * Time.deltaTime;
            tireTransform.position = Vector3.Lerp(tireTransform.position, tireDesiredPosition, movementSpeed);

            if (canSteer)
            {
                if(tireTransform.localEulerAngles.y != pair.Wheel.SteerAngle)
                {
                    tireTransform.localRotation = Quaternion.Euler(tireTransform.localRotation.eulerAngles.x, pair.Wheel.SteerAngle, tireTransform.localRotation.eulerAngles.z);
                }
            }

            wheelReposition.DummiesMovement(tireTransform, pair, tireDesiredPosition, movementSpeed);
        }




    }
}
