using Frontend.Scripts.Enums;
using Frontend.Scripts.Interfaces;
using Frontend.Scripts.Models;
using Frontend.Scripts.ScriptableObjects;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Frontend.Scripts.Components
{
    public class UTSuspensionController : MonoBehaviour, IVehicleController
    {
        [Inject(Id = "mainRig")] private Rigidbody rig;
        [Inject] private readonly UTAxle[] allAxles;
        [Inject] private readonly GameParameters gameParameters;

        [SerializeField] private float maxSlopeAngle = 45f;
        [SerializeField] private Text velocityText;
        [SerializeField] private Transform com;
        [SerializeField] private AnimationCurve comSteeringCurve;
        [SerializeField] private AnimationCurve angleBasedComSteeringCurve;
        [SerializeField] private AnimationCurve enginePowerCurve;
        [SerializeField] private bool airControl = true;

        private bool isBrake;
        private bool hasAnyWheels;
        private float inputX, inputY;
        private float absoluteInputY, absoluteInputX;
        private float signedInputY;
        private float currentSpeed;
        private float currentDriveForce = 0;
        private float currentLongitudalGrip;
        private float forwardForce, turnForce;
        private Vector3 wheelVelocityLocal;
        private int allWheelsAmount=0;

        private IEnumerable<UTWheel> allGroundedWheels;
        private UTWheel[] allWheels;
        public UTAxle[] AllAxles => allAxles;
        public bool HasAnyWheels => hasAnyWheels;
        public float CurrentSpeed => currentSpeed;
        public float AbsoluteInputY => absoluteInputY;
        public float AbsoluteInputX => absoluteInputX;
        public float SignedInputY => signedInputY;


        public void Initialize()
        {
            hasAnyWheels = allAxles.Any() && allAxles.Where(axle => axle.HasAnyWheelPair).Any();
            rig.centerOfMass = com.localPosition;

            allWheels = GetAllWheelsInAllAxles().ToArray();
            allWheelsAmount = allWheels.Length;
        }

        private void Update()
        {
            isBrake = Input.GetKey(KeyCode.Space);
            inputY = !isBrake ? Input.GetAxis("Vertical") : 0f;
            inputX = Input.GetAxis("Horizontal");

            absoluteInputY = Mathf.Abs(inputY);
            absoluteInputX = Mathf.Abs(inputX) * Mathf.Sign(inputY);

            signedInputY = Mathf.Sign(inputY);

            velocityText.text = $"{currentSpeed:F0}";
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

            SetSpeedometr(rig.velocity.magnitude * 4f);
        }

        private void EvaluateDriveParams()
        {
            currentDriveForce = enginePowerCurve.Evaluate(currentSpeed);
        }

        private void SetSpeedometr(float speed)
        {
            currentSpeed = speed;
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
