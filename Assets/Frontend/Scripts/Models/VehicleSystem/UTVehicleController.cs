using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;
using Frontend.Scripts.Interfaces;
using Frontend.Scripts.Models;
using GLShared.General.Enums;
using GLShared.General.ScriptableObjects;
using GLShared.General.Interfaces;
using Frontend.Scripts.Components;
using Frontend.Scripts.Extensions;

namespace Frontend.Scripts.Models
{
    public abstract class UTVehicleController : MonoBehaviour, IVehicleController
    {
        [Inject(Id = "mainRig")] protected Rigidbody rig;
        [Inject] protected readonly IEnumerable<IVehicleAxle> allAxles;
        [Inject] protected readonly GameParameters gameParameters;
        [Inject] protected readonly IPlayerInputProvider inputProvider;
        [Inject] protected readonly VehicleStatsBase vehicleStats;
        [Inject(Optional = true)] protected readonly Speedometer speedometer;

        [SerializeField] protected Transform centerOfMass;
        [SerializeField] protected VehicleType vehicleType = VehicleType.Car;
        [SerializeField] protected float maxSlopeAngle = 45f;
        [SerializeField] protected AnimationCurve enginePowerCurve;
        [SerializeField] protected bool doesGravityDamping = true;

        [Header("Force apply points")]
        [SerializeField] protected ForceApplyPoint brakesForceApplyPoint = ForceApplyPoint.WheelConstraintUpperPoint;
        [SerializeField] protected ForceApplyPoint accelerationForceApplyPoint = ForceApplyPoint.WheelHitPoint;

        [SerializeField] protected float visualElementsMovementSpeed = 50f;

        protected bool hasAnyWheels;
        protected float currentSpeed;
        protected float absoluteInputY;
        protected float absoluteInputX;
        protected float maxForwardSpeed;
        protected float maxBackwardsSpeed;
        protected float currentSpeedRatio;
        protected float signedInputY;
        protected int allWheelsAmount = 0;

        #region Computed variables
        protected bool isBrake;
        protected float inputY;
        protected float currentDriveForce = 0;
        protected float currentLongitudalGrip;
        protected float forwardForce;
        protected float turnForce;
        protected bool isUpsideDown = false;
        protected Vector3 wheelVelocityLocal;
        #endregion 

        protected IEnumerable<IPhysicsWheel> allGroundedWheels;
        protected IEnumerable<IPhysicsWheel> allWheels;

        public VehicleType VehicleType => vehicleType;
        public IEnumerable<IVehicleAxle> AllAxles => allAxles;
        public bool HasAnyWheels => hasAnyWheels;
        public float CurrentSpeed => currentSpeed;
        public float CurrentSpeedRatio =>  currentSpeedRatio;
        public float AbsoluteInputY => absoluteInputY;
        public float AbsoluteInputX => absoluteInputX;
        public float SignedInputY => signedInputY;
        public float MaxForwardSpeed => maxForwardSpeed;
        public float MaxBackwardsSpeed => maxBackwardsSpeed;
        public bool DoesGravityDamping => doesGravityDamping;
        public bool IsUpsideDown => isUpsideDown;

        public ForceApplyPoint BrakesForceApplyPoint => brakesForceApplyPoint;
        public ForceApplyPoint AccelerationForceApplyPoint => accelerationForceApplyPoint;

        public IEnumerable<IPhysicsWheel> AllWheels => allWheels;
        public float VisualElementsMovementSpeed => visualElementsMovementSpeed;
        public float GetCurrentMaxSpeed()
        {
            return absoluteInputY == 0 ? 0 : (signedInputY > 0 ? maxForwardSpeed : maxBackwardsSpeed);
        }

        public virtual void Initialize()
        {
            SetupRigidbody();

            maxForwardSpeed = enginePowerCurve.keys[enginePowerCurve.keys.Length - 1].time;
            maxBackwardsSpeed = maxForwardSpeed / 2f;

            hasAnyWheels = allAxles.Any() && allAxles.Where(axle => axle.HasAnyWheelPair && axle.HasAnyWheel).Any();
            allWheels = GetAllWheelsInAllAxles().ToArray();
            allWheelsAmount = allWheels.Count();
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

        protected virtual void FixedUpdate()
        {
            allGroundedWheels = GetGroundedWheelsInAllAxles().ToArray();
            isUpsideDown = CheckUpsideDown();
        }

        protected virtual void Update()
        {
            if (inputProvider != null)
            {
                isBrake = inputProvider.Brake;
                inputY = inputProvider.Vertical;

                absoluteInputY = inputProvider.AbsoluteVertical;
                absoluteInputX = inputProvider.AbsoluteHorizontal;

                signedInputY = inputProvider.SignedVertical;
            }
        }
        
        protected void SetCurrentSpeed()
        {
            currentSpeed = rig.velocity.magnitude * gameParameters.SpeedMultiplier;
            float maxSpeed = GetCurrentMaxSpeed();
            currentSpeedRatio = maxSpeed != 0 ? currentSpeed / maxSpeed : 0f;
        }

        protected bool CheckUpsideDown()
        {
            return !allGroundedWheels.Any() && transform.up.y <= 0f;
        }

        protected void EvaluateDriveParams()
        {
            currentDriveForce = enginePowerCurve.Evaluate(currentSpeed);
        }

        protected void Accelerate()
        {
            if (absoluteInputY == 0 || isBrake)
            {
                return;
            }

            foreach (var axle in allAxles)
            {
                if (axle.CanDrive && !isBrake)
                {
                    var groundedWheels = axle.GroundedWheels;

                    if (!groundedWheels.Any())
                    {
                        continue;
                    }

                    foreach (var wheel in groundedWheels)
                    {
                        if (wheel.HitInfo.NormalAndUpAngle <= gameParameters.MaxWheelDetectionAngle)
                        {
                            wheelVelocityLocal = wheel.Transform.InverseTransformDirection(rig.GetPointVelocity(wheel.UpperConstraintPoint));
                         
                            forwardForce = inputY * currentDriveForce;
                            turnForce = wheelVelocityLocal.x * currentDriveForce;

                            Vector3 acceleratePoint = wheel.ReturnWheelPoint(accelerationForceApplyPoint);

                            rig.AddForceAtPosition((forwardForce * wheel.Transform.forward), acceleratePoint);
                            rig.AddForceAtPosition((turnForce * -wheel.Transform.right), wheel.UpperConstraintPoint);
                        }
                    }
                }
            }
        }

        protected void Brakes()
        {
            if (!allGroundedWheels.Any())
            {
                return;
            }

            currentLongitudalGrip = isBrake ? 1f : (absoluteInputY > 0 ? 0 : 0.5f);

            if (absoluteInputY == 0 || isBrake)
            {
                foreach (var wheel in allGroundedWheels)
                {
                    Vector3 brakesPoint = wheel.ReturnWheelPoint(brakesForceApplyPoint);

                    Vector3 forwardDir = wheel.Transform.forward;
                    Vector3 tireVel = rig.GetPointVelocity(brakesPoint);

                    float steeringVel = Vector3.Dot(forwardDir, tireVel);
                    float desiredVelChange = -steeringVel * currentLongitudalGrip;
                    float desiredAccel = desiredVelChange / Time.fixedDeltaTime;

                    rig.AddForceAtPosition(desiredAccel * wheel.TireMass/2f * forwardDir, brakesPoint);
                }
            }
        }
        protected virtual void CustomGravityLogic()
        {
        }

        protected IEnumerable<IPhysicsWheel> GetGroundedWheelsInAllAxles()
        {
            var result = new List<IPhysicsWheel>();
            if (allAxles.Any())
            {
                foreach (var axle in allAxles)
                {
                    if(axle.GroundedWheels.Any())
                    {
                        result.AddRange(axle.GroundedWheels);
                    }
                }
            }
            return result;
        }

        protected IEnumerable<IPhysicsWheel> GetAllWheelsInAllAxles()
        {
            var result = new List<IPhysicsWheel>();
            if (allAxles.Any())
            {
                foreach (var axle in allAxles)
                {
                    if(axle.HasAnyWheelPair && axle.HasAnyWheel)
                    {
                        result.AddRange(axle.AllWheels);
                    }
                }
            }
            return result;
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
