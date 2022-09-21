using Frontend.Scripts.Enums;
using Frontend.Scripts.Models;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Frontend.Scripts.Components
{
    public class UTSuspensionController : MonoBehaviour
    {
        [SerializeField] private Rigidbody rig;
        [SerializeField] private float maxSlopeAngle = 45f;
        [SerializeField] private UTWheel[] allWheels;
        [SerializeField] private Text velocityText;
        [SerializeField] private Transform com;
        [SerializeField] private AnimationCurve comSteeringCurve;
        [SerializeField] private AnimationCurve angleBasedComSteeringCurve;
        [SerializeField] private AnimationCurve enginePowerCurve;

        private bool isBrake;
        private float inputX, inputY;
        private float absoluteInputY, absoluteInputX;
        private float currentSpeed;
        private float currentDriveForce = 0;
        private float currentLongitudalGrip;
        private float Fx, Fy;
        private Vector3 wheelVelocityLocal;

        private void Awake()
        {
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
            //CustomGravityLogic();
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
            foreach (var wheel in allWheels)
            {
                if (wheel.IsGrounded)
                {
                    Vector3 steeringDir = wheel.transform.right;
                    Vector3 tireVel = rig.GetPointVelocity(wheel.HitInfo.Point);

                    float steeringVel = Vector3.Dot(steeringDir, tireVel);
                    float desiredVelChange = -steeringVel * wheel.SidewaysTireGripFactor;
                    float desiredAccel = desiredVelChange / Time.fixedDeltaTime;

                    rig.AddForceAtPosition(desiredAccel * wheel.TireMass * steeringDir, wheel.HitInfo.Point);
                }
            }
        }

        private void Accelerate()
        {
            foreach (var wheel in allWheels)
            {
                if (wheel.CanDrive & wheel.IsGrounded && !isBrake)
                {
                    wheelVelocityLocal = wheel.transform.InverseTransformDirection(rig.GetPointVelocity(wheel.transform.position));

                    Fx = inputY * currentDriveForce;
                    Fy = wheelVelocityLocal.x * currentDriveForce;

                    rig.AddForceAtPosition((Fx * wheel.transform.forward), wheel.HitInfo.Point);
                    rig.AddForceAtPosition((Fy * -wheel.transform.right), wheel.transform.position);
                }
            }
        }


        private void Brakes()
        {
            currentLongitudalGrip = isBrake ? 1f : (absoluteInputY > 0 ? 0 : 0.5f);
            foreach (var wheel in allWheels)
            {
                if (wheel.IsGrounded)
                {
                    Vector3 forwardDir = wheel.transform.forward;
                    Vector3 tireVel = rig.GetPointVelocity(wheel.transform.position);

                    float steeringVel = Vector3.Dot(forwardDir, tireVel);
                    float desiredVelChange = -steeringVel * currentLongitudalGrip;
                    float desiredAccel = desiredVelChange / Time.fixedDeltaTime;

                    rig.AddForceAtPosition(desiredAccel * wheel.TireMass * forwardDir, wheel.transform.position);
                }
            }

        }

        private void CustomGravityLogic()
        {
            foreach (var wheel in allWheels)
            {
                if (wheel.IsGrounded)
                {
                    float angle = Vector3.Angle(wheel.HitInfo.Normal, -Physics.gravity.normalized);

                    if (maxSlopeAngle >= angle)
                    {
                        rig.AddForce(-wheel.HitInfo.Normal * Physics.gravity.magnitude, ForceMode.Acceleration);
                        break;
                    }
                }
                rig.AddForce(Physics.gravity, ForceMode.Acceleration);
            }
        }
    }
}
