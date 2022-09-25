using Frontend.Scripts.Enums;
using Frontend.Scripts.Models;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Frontend.Scripts.Components
{
    public class TestSuspension : MonoBehaviour
    {
        [SerializeField] private HoverSpring[] allWheels;
        [SerializeField] private Rigidbody rig;
        [SerializeField] private Transform com;
        [SerializeField] private Text velocityText;
        
        [SerializeField] private AnimationCurve enginePowerCurve;
        [SerializeField] private float maximumComLowering = -1.4f;

        [Header("-Vehicle stablization-")]
        [SerializeField] private AnimationCurve velocityComCurve;
        [SerializeField] private AnimationCurve angleComCurve;

        private float inputX, inputY;
        private float absoluteInputY, absoluteInputX;
        private float currentSpeed;
        private float horizontalAngle = 0;
        private float currentDriveForce = 0;
        public float angleVelocityCombinatedEvaluation;
        public float maxSlopeAngle = 45f;

        private Vector3 wheelVelocityLocal;
        private float Fx, Fy;
        public bool isBrake;
        public float currentLongitudalGrip;

        public HoverSpring[] AllWheels => allWheels;
        public float CurrentSpeed => currentSpeed;

        public void PassInputs(float inputX, float inputY, float absoluteX, float absoluteY)
        {
            this.inputX = inputX;
            this.inputY = inputY;
            this.absoluteInputX = absoluteX;
            this.absoluteInputY = absoluteY;
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
            AntirollCOM();
            currentSpeed = rig.velocity.magnitude * 3.6f;
            horizontalAngle = Mathf.Abs(90f - Vector3.Angle(Vector3.up, transform.right));
        }

        private void EvaluateDriveParams()
        {
            currentDriveForce = enginePowerCurve.Evaluate(currentSpeed);
        }

        private void AntirollCOM()
        {
            float evaluatedByAngle = angleComCurve.Evaluate(horizontalAngle);
            float evaluatedByVelocity = velocityComCurve.Evaluate(currentSpeed);
            angleVelocityCombinatedEvaluation = Mathf.Clamp(evaluatedByAngle + evaluatedByVelocity, + maximumComLowering, 0);
            rig.centerOfMass = (com.localPosition + new Vector3(0, angleVelocityCombinatedEvaluation, 0));
        }

        private void CustomGravityLogic()
        {
            foreach (var wheel in allWheels)
            {
                if (wheel.IsGrounded)
                {
                    float angle = Vector3.Angle(wheel.HitInfo.Normal, -Physics.gravity.normalized);

                    if(maxSlopeAngle >= angle)
                    {
                        rig.AddForce(-wheel.HitInfo.Normal * Physics.gravity.magnitude, ForceMode.Acceleration);
                        break;
                    }
                }
                rig.AddForce(Physics.gravity, ForceMode.Acceleration);
            }
        }

        private void Accelerate()
        {
            foreach (var wheel in allWheels)
            {
                if (wheel.canDrive & wheel.IsGrounded && !isBrake)
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

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.white;
            Gizmos.DrawSphere(rig.worldCenterOfMass, 0.2f);
        }
    }
}