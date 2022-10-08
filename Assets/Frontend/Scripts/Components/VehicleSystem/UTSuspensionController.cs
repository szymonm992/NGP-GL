using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;
using Frontend.Scripts.Interfaces;
using Frontend.Scripts.Models;
using GLShared.General.Enums;
using GLShared.General.ScriptableObjects;

namespace Frontend.Scripts.Components
{
    public class UTSuspensionController : MonoBehaviour, IVehicleController
    {
        [Inject(Id = "mainRig")] private Rigidbody rig;
        [Inject] private readonly IEnumerable<UTAxle> allAxles;
        [Inject] private readonly GameParameters gameParameters;
        [Inject] private readonly IPlayerInputProvider inputProvider;
        [Inject] private readonly VehicleStatsBase vehicleStats;
        [Inject (Optional = true)] private readonly Speedometer speedometer;

        [SerializeField] private float maxSlopeAngle = 45f;
        [SerializeField] private Transform centerOfMass;
        [SerializeField] private AnimationCurve enginePowerCurve;
        [SerializeField] private bool airControl = true;

        private int allWheelsAmount = 0;
        private bool hasAnyWheels;

        private bool isBrake;
        private float inputY;
        private float absoluteInputY;
        private float signedInputY;

        private float currentSpeed;
        private float currentDriveForce = 0;
        private float currentLongitudalGrip;
        private float forwardForce;
        private float turnForce;
        private float maxForwardSpeed;
        private float maxBackwardsSpeed;
        private Vector3 wheelVelocityLocal;
        
        private IEnumerable<UTWheel> allGroundedWheels;
        private UTWheel[] allWheels;

        public VehicleType VehicleType => VehicleType.Car;
        public IEnumerable<UTAxle> AllAxles => allAxles;
        public bool HasAnyWheels => hasAnyWheels;
        public float CurrentSpeed => currentSpeed;
        public float AbsoluteInputY => absoluteInputY;
        public float MaxForwardSpeed => maxForwardSpeed;
        public float MaxBackwardsSpeed => maxBackwardsSpeed;


        public void Initialize()
        {
            SetupRigidbody();

            maxForwardSpeed = enginePowerCurve.keys[enginePowerCurve.keys.Length-1].time;
            maxBackwardsSpeed = maxForwardSpeed/2;

            hasAnyWheels = allAxles.Any() && allAxles.Where(axle => axle.HasAnyWheelPair).Any();
            allWheels = GetAllWheelsInAllAxles().ToArray();
            allWheelsAmount = allWheels.Length;
        }

        public void SetupRigidbody()
        {
            rig.mass = vehicleStats.Mass;
            rig.drag = vehicleStats.Drag;
            rig.angularDrag = vehicleStats.AngularDrag;
            if(centerOfMass!= null)
            {
                rig.centerOfMass = centerOfMass.localPosition;
            }
        }

        public float GetCurrentMaxSpeed()
        {
            return signedInputY == 0 ? 0 : (signedInputY > 0 ? maxForwardSpeed : maxBackwardsSpeed);
        }

        private void Update()
        {
            if (inputProvider != null)
            {
                isBrake = inputProvider.Brake;
                inputY = inputProvider.Vertical;

                absoluteInputY = inputProvider.AbsoluteVertical;

                signedInputY = inputProvider.SignedVertical;
            }
        }

        private void FixedUpdate()
        {
            allGroundedWheels = GetGroundedWheelsInAllAxles();

            CustomGravityLogic();
            EvaluateDriveParams();
            Accelerate();
            Brakes();
            ApplyFrictionForces();
            AirControl();

            currentSpeed = rig.velocity.magnitude * gameParameters.SpeedMultiplier;
            speedometer?.SetSpeedometr(currentSpeed);
        }

        private void EvaluateDriveParams()
        {
            currentDriveForce = enginePowerCurve.Evaluate(currentSpeed);
        }

        private void ApplyFrictionForces()
        {
            if(!allGroundedWheels.Any())
            {
                return;
            }

            foreach (var wheel in allGroundedWheels)
            {
                Vector3 steeringDir = wheel.transform.right;
                Vector3 tireVel = rig.GetPointVelocity(wheel.HitInfo.Point);

                float steeringVel = Vector3.Dot(steeringDir, tireVel);
                float desiredVelChange = -steeringVel * wheel.SidewaysTireGripFactor;
                float desiredAccel = desiredVelChange / Time.fixedDeltaTime;

                rig.AddForceAtPosition(desiredAccel * wheel.TireMass * steeringDir, wheel.HitInfo.Point);
            }
        }

        private void Accelerate()
        {
            if (absoluteInputY == 0 || isBrake)
            {
                return;
            }

            foreach (var axle in allAxles)
            {
                if (axle.CanDrive && !isBrake)
                {
                    var groundedWheels = axle.GetGroundedWheels();

                    if (!groundedWheels.Any())
                    {
                        return;
                    }

                    foreach (var wheel in groundedWheels)
                    {
                        if (wheel.HitInfo.NormalAndUpAngle <= gameParameters.MaxWheelDetectionAngle)
                        {
                            wheelVelocityLocal = wheel.transform.InverseTransformDirection(rig.GetPointVelocity(wheel.UpperConstraintPoint));

                            forwardForce = inputY * currentDriveForce;
                            turnForce = wheelVelocityLocal.x * currentDriveForce;

                            rig.AddForceAtPosition((forwardForce * wheel.transform.forward), wheel.HitInfo.Point);
                            rig.AddForceAtPosition((turnForce * -wheel.transform.right), wheel.UpperConstraintPoint);
                        }
                    }
                }
            }
        }


        private void Brakes()
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
                    Vector3 forwardDir = wheel.transform.forward;
                    Vector3 tireVel = rig.GetPointVelocity(wheel.UpperConstraintPoint);

                    float steeringVel = Vector3.Dot(forwardDir, tireVel);
                    float desiredVelChange = -steeringVel * currentLongitudalGrip;
                    float desiredAccel = desiredVelChange / Time.fixedDeltaTime;

                    rig.AddForceAtPosition(desiredAccel * wheel.TireMass * forwardDir, wheel.UpperConstraintPoint);
                }
            }
        }

        private void CustomGravityLogic()
        {
            int groundedWheelsAmount = allGroundedWheels.Count();
            if (!allGroundedWheels.Any())
            {
                rig.AddForce(Physics.gravity, ForceMode.Acceleration);
            }
            else if (allWheelsAmount == groundedWheelsAmount)
            {
                float angle = Vector3.Angle(transform.up, -Physics.gravity.normalized);

                if (maxSlopeAngle >= angle)
                {
                    rig.AddForce(-transform.up * Physics.gravity.magnitude, ForceMode.Acceleration);
                }
                else
                {
                    rig.AddForce(Physics.gravity, ForceMode.Acceleration);
                }
            }
            else
            {
                var notGroundedAmount = (allWheelsAmount - groundedWheelsAmount);
                foreach (var wheel in allWheels)
                {
                    if (!wheel.IsGrounded)
                    {
                        rig.AddForceAtPosition((Physics.gravity / notGroundedAmount), wheel.UpperConstraintPoint, ForceMode.Acceleration);
                    }
                }
            }
        }

        private void AirControl()
        {
            if (!airControl)
            {
                return;
            }

            if (!allGroundedWheels.Any())
            {
                float angle = transform.eulerAngles.x;
                angle = (angle > 180) ? angle - 360 : angle;

                if (Mathf.Abs(angle) > gameParameters.AirControlAngleThreshold)
                {
                    rig.AddTorque(transform.right * (gameParameters.AirControlForce * -Mathf.Sign(angle)), ForceMode.Acceleration);
                }
            }
        }

        private IEnumerable<UTWheel> GetGroundedWheelsInAllAxles()
        {
            var result = new List<UTWheel>();
            if(allAxles.Any())
            {
                foreach (var axle in allAxles)
                {
                    result.AddRange(axle.GetGroundedWheels());
                }
            }
            return result;
        } 
        
        private IEnumerable<UTWheel> GetAllWheelsInAllAxles()
        {
            var result = new List<UTWheel>();
            if(allAxles.Any())
            {
                foreach (var axle in allAxles)
                {
                    result.AddRange(axle.GetAllWheels());
                }
            }
            return result;
        }


        private void OnDrawGizmos()
        {
            #if UNITY_EDITOR
            if(rig == null)
            {
                rig = GetComponent<Rigidbody>();
            }

            Gizmos.color = Color.white;
            Gizmos.DrawSphere(rig.worldCenterOfMass, 0.2f);
            #endif
        }

        
    }
}
