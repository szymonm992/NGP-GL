using Frontend.Scripts.Components;
using Frontend.Scripts.Interfaces;
using GLShared.General.Enums;
using GLShared.General.Interfaces;
using GLShared.General.Models;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Zenject;

namespace Frontend.Scripts.Models
{
    public abstract class UTAxleBase : MonoBehaviour, IInitializable, IVehicleAxle
    {
        public const float SUSPENSION_VISUALS_MOVEMENT_SPEED = 50F;

        [Inject(Id = "mainRig")] protected readonly Rigidbody rig;
        [Inject] protected readonly IVehicleController controller;
        [Inject] protected readonly IPlayerInputProvider inputProvider;
        [Inject] protected readonly IWheelReposition wheelReposition;
        [Inject] protected readonly VehicleModelEffects vehicleModelEffects;

        [SerializeField] protected UTAxlePair[] wheelPairs;
        [SerializeField] protected bool canDrive;
        [SerializeField] protected bool canSteer;
        [SerializeField] protected bool invertSteer = false;

        
        [SerializeField] protected bool repositionVisuals = true;

        public UTAxleDebug debugSettings = new UTAxleDebug()
        {
            DrawGizmos = true,
            DrawAxleCenter = true,
            DrawAxlePipes = true,
            DrawMode = UTDebugMode.All
        };

        
        protected IEnumerable<IPhysicsWheel> allWheels;
        protected IEnumerable<IPhysicsWheel> groundedWheels;
        protected bool hasAnyWheel = false;

        public IEnumerable<UTAxlePair> WheelPairs => wheelPairs;
        public IEnumerable<IPhysicsWheel> AllWheels => allWheels;
        public IEnumerable<IPhysicsWheel> GroundedWheels => groundedWheels;
        public bool CanDrive => canDrive;
        public bool CanSteer => canSteer;
        public bool InvertSteer => invertSteer;
        public bool HasAnyWheelPair => wheelPairs.Any();
        public bool HasAnyWheel => hasAnyWheel;

        protected void Awake()
        {
            allWheels = wheelPairs.Select(pair => pair.Wheel).ToArray();
            hasAnyWheel = allWheels.Any();
        }

        protected IEnumerable<IPhysicsWheel> GetGroundedWheels()
        {
            return allWheels.Where(wheel => wheel.IsGrounded == true);
        }

        public IEnumerable<IPhysicsWheel> GetAllWheelsOfAxis(DriveAxisSite axis)
        {
            return wheelPairs.Where(pair => pair.Axis == axis).Select(pair => pair.Wheel).ToArray();
        }

        public virtual void Initialize()
        {
            if (HasAnyWheelPair)
            {
                foreach (var pair in wheelPairs)
                {
                    pair.Initialize();
                }
            }

            groundedWheels = GetGroundedWheels();
        }

        public virtual void SetSteerAngle(float angleLeft, float angleRight)
        {
        }

        protected virtual void RepositionTireModel(UTAxlePair pair)
        {

        }

        protected void OnDrawGizmos()
        {
            #if UNITY_EDITOR
            bool drawCurrently = (debugSettings.DrawGizmos) && (debugSettings.DrawMode == UTDebugMode.All)
               || (debugSettings.DrawMode == UTDebugMode.EditorOnly && !Application.isPlaying)
               || (debugSettings.DrawMode == UTDebugMode.PlaymodeOnly && Application.isPlaying);

            if (drawCurrently)
            {
                Gizmos.color = Color.white;
                if (debugSettings.DrawAxleCenter)
                {
                    Gizmos.DrawSphere(transform.position, .11f);
                }

                if (debugSettings.DrawAxlePipes)
                {
                    if (wheelPairs == null || !wheelPairs.Any())
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
