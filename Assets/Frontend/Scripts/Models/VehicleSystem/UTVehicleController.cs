using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;
using Frontend.Scripts.Interfaces;
using Frontend.Scripts.Models;
using GLShared.General.Enums;
using GLShared.General.ScriptableObjects;
using Frontend.Scripts.Components;

namespace Frontend.Scripts.Models
{
    public abstract class UTVehicleController : MonoBehaviour, IVehicleController
    {
        [Inject(Id = "mainRig")] protected Rigidbody rig;
        [Inject] protected readonly IEnumerable<UTAxle> allAxles;
        [Inject] protected readonly GameParameters gameParameters;
        [Inject] protected readonly IPlayerInputProvider inputProvider;
        [Inject] protected readonly VehicleStatsBase vehicleStats;
        [Inject(Optional = true)] protected readonly Speedometer speedometer;

        [SerializeField] protected Transform centerOfMass;
        [SerializeField] protected VehicleType vehicleType = VehicleType.Car;
        [SerializeField] protected float maxSlopeAngle = 45f;
        [SerializeField] protected AnimationCurve enginePowerCurve;

        protected bool hasAnyWheels;
        protected float currentSpeed;
        protected float absoluteInputY;
        protected float maxForwardSpeed;
        protected float maxBackwardsSpeed;
        protected float signedInputY;
        protected int allWheelsAmount = 0;

        protected IEnumerable<UTWheel> allGroundedWheels;
        protected UTWheel[] allWheels;

        public VehicleType VehicleType => vehicleType;
        public IEnumerable<UTAxle> AllAxles => allAxles;
        public bool HasAnyWheels => hasAnyWheels;
        public float CurrentSpeed => currentSpeed;
        public float AbsoluteInputY => absoluteInputY;
        public float SignedInputY => signedInputY;
        public float MaxForwardSpeed => maxForwardSpeed;
        public float MaxBackwardsSpeed => maxBackwardsSpeed;

        public virtual void Initialize()
        {
            SetupRigidbody();

            maxForwardSpeed = enginePowerCurve.keys[enginePowerCurve.keys.Length - 1].time;
            maxBackwardsSpeed = maxForwardSpeed / 2f;

            hasAnyWheels = allAxles.Any() && allAxles.Where(axle => axle.HasAnyWheelPair).Any();
            allWheels = GetAllWheelsInAllAxles().ToArray();
            allWheelsAmount = allWheels.Length;
        }

        public virtual void SetupRigidbody()
        {
            rig.mass = vehicleStats.Mass;
            rig.drag = vehicleStats.Drag;
            rig.angularDrag = vehicleStats.AngularDrag;
            if (centerOfMass != null)
            {
                rig.centerOfMass = centerOfMass.localPosition;
            }
        }

        protected IEnumerable<UTWheel> GetGroundedWheelsInAllAxles()
        {
            var result = new List<UTWheel>();
            if (allAxles.Any())
            {
                foreach (var axle in allAxles)
                {
                    result.AddRange(axle.GetGroundedWheels());
                }
            }
            return result;
        }

        protected IEnumerable<UTWheel> GetAllWheelsInAllAxles()
        {
            var result = new List<UTWheel>();
            if (allAxles.Any())
            {
                foreach (var axle in allAxles)
                {
                    result.AddRange(axle.GetAllWheels());
                }
            }
            return result;
        }

        public float GetCurrentMaxSpeed()
        {
            return signedInputY == 0 ? 0 : (signedInputY > 0 ? maxForwardSpeed : maxBackwardsSpeed);
        }

        private void OnDrawGizmos()
        {
            #if UNITY_EDITOR
            if (rig == null)
            {
                rig = GetComponent<Rigidbody>();
            }

            Gizmos.color = Color.white;
            Gizmos.DrawSphere(rig.worldCenterOfMass, 0.2f);
            #endif
        }
    }
}