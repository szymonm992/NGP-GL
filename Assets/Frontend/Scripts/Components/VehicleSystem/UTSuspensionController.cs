using Frontend.Scripts.Enums;
using Frontend.Scripts.Interfaces;
using Frontend.Scripts.Models;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Frontend.Scripts.Components
{
    public class UTSuspensionController : MonoBehaviour, IVehicleController
    {
        [SerializeField] private Rigidbody rig;
        [SerializeField] private float maxSlopeAngle = 45f;
        [SerializeField] private UTAxle[] allAxles;
        [SerializeField] private Text velocityText;
        [SerializeField] private Transform com;
        [SerializeField] private AnimationCurve comSteeringCurve;
        [SerializeField] private AnimationCurve angleBasedComSteeringCurve;
        [SerializeField] private AnimationCurve enginePowerCurve;

        private bool isBrake;
        private bool hasAnyWheels;
        private float inputX, inputY;
        private float absoluteInputY, absoluteInputX;
        private float currentSpeed;
        private float currentDriveForce = 0;
        private float currentLongitudalGrip;
        private float Fx, Fy;
        private Vector3 wheelVelocityLocal;

        public UTAxle[] AllAxles => allAxles;
        public bool HasAnyWheels => hasAnyWheels;
        public float CurrentSpeed => currentSpeed;

        private void Awake()
        {
            hasAnyWheels = allAxles.Any() && allAxles.Where(axle => axle.HasAnyWheelPair).Any();
            rig.centerOfMass = com.localPosition;
        }
        private void Update()
        {
            isBrake = Input.GetKey(KeyCode.Space);
            inputY = !isBrake ? Input.GetAxis("Vertical") : 0f;
            inputX = Input.GetAxis("Horizontal");

            absoluteInputY = Mathf.Abs(inputY);
            absoluteInputX = Mathf.Abs(inputX);

            velocityText.text = currentSpeed.ToString("F0");
        }

        private void FixedUpdate()
        {
            CustomGravityLogic();
            EvaluateDriveParams();
            Accelerate();
            Brakes();
            ApplyFrictionForces();
            currentSpeed = rig.velocity.magnitude * 3.6f;
        }

        private void EvaluateDriveParams()
        {
            currentDriveForce = enginePowerCurve.Evaluate(currentSpeed);
        }


        private void ApplyFrictionForces()
        {
            var allGroundedWheels = GetGroundedWheelsInAllAxles();
            foreach (var wheel in allGroundedWheels)
            {
                Vector3 steeringDir = wheel.transform.right;
                wheel.GetWorldPosition(out var worldPos, out _);
                Vector3 tireVel = rig.GetPointVelocity(worldPos);

                float steeringVel = Vector3.Dot(steeringDir, tireVel);
                float desiredVelChange = -steeringVel * wheel.SidewaysTireGripFactor;
                float desiredAccel = desiredVelChange / Time.fixedDeltaTime;

                rig.AddForceAtPosition(desiredAccel * wheel.TireMass * steeringDir, worldPos);
            }

        }

        private void Accelerate()
        {
            foreach (var axle in allAxles)
            {
                if (axle.CanDrive && !isBrake)
                {
                    var groundedWheels = axle.GetGroundedWheels();
                    foreach (var wheel in groundedWheels)
                    {
                        wheelVelocityLocal = wheel.transform.InverseTransformDirection(rig.GetPointVelocity(wheel.transform.position));

                        Fx = inputY * currentDriveForce;
                        Fy = wheelVelocityLocal.x * currentDriveForce;

                        rig.AddForceAtPosition((Fx * wheel.transform.forward), wheel.HitInfo.Point);
                        rig.AddForceAtPosition((Fy * -wheel.transform.right), wheel.transform.position);
                    }
                }
            }
        }


        private void Brakes()
        {
            currentLongitudalGrip = isBrake ? 1f : (absoluteInputY > 0 ? 0 : 0.5f);

            var allGroundedWheels = GetGroundedWheelsInAllAxles();
            foreach (var wheel in allGroundedWheels)
            {
                Vector3 forwardDir = wheel.transform.forward;
                Vector3 tireVel = rig.GetPointVelocity(wheel.transform.position);

                float steeringVel = Vector3.Dot(forwardDir, tireVel);
                float desiredVelChange = -steeringVel * currentLongitudalGrip;
                float desiredAccel = desiredVelChange / Time.fixedDeltaTime;

                rig.AddForceAtPosition(desiredAccel * wheel.TireMass * forwardDir, wheel.transform.position);
            }


        }

        private void CustomGravityLogic()
        {
            var allGroundedWheels = GetGroundedWheelsInAllAxles();
            if(!allGroundedWheels.Any())
            {
                rig.AddForce(Physics.gravity, ForceMode.Acceleration);
                return;
            }

            foreach (var wheel in allGroundedWheels)
            {
                float angle = Vector3.Angle(wheel.HitInfo.Normal, -Physics.gravity.normalized);

                if (maxSlopeAngle >= angle)
                {
                    rig.AddForce(-wheel.HitInfo.Normal * Physics.gravity.magnitude, ForceMode.Acceleration);
                    break;
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
    }
}
