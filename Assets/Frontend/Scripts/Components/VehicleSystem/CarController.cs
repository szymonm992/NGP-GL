using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using Frontend.Scripts.Models.VehicleSystem;
using Frontend.Scripts.Enums;
using Frontend.Scripts.Interfaces;

namespace Frontend.Scripts.Components
{
   
    
   
    public class CarController : MonoBehaviour, IInitializable, IVehicleController
    {

        [Inject] public readonly CarStats carStats;
        [Inject] public readonly IPlayerInput playerInputs;

        [Inject(Id = "mainRig")] private readonly Rigidbody rig;

        [Header("GUI")]
        [SerializeField] private Text speedometer;

        [Header("Detectors")]
        [SerializeField] private Transform groundCheck;
        [SerializeField] private Transform centerOfMass;

        [Space]
        [SerializeField] private DriveElement[] driveElements;
        [SerializeField] private float groundingRayLength = 2f;

        #region LOCAL CONTROL VARIABLES
        private float speedValue, fricValue, turnValue, brakeInput;

        private Vector3 carVelocity = Vector2.zero;
        
        private float currentSpeed = 0f;
        private float terrainAngle = 0f;
        
        private float currentSpeedRatio = 0f;

        private int wheelsAmount=0;

        private bool grounded = false;
        private bool isLocalBraking = false;
        private bool isSetup=false;
        #endregion

        private void Update()
        {
            if (!isSetup) return;
            ReadInputs();
            grounded = IsGrounded();

            VisualEffects();

            
        }
        public void Initialize()
        {
            if (centerOfMass)
            {
                rig.centerOfMass = centerOfMass.localPosition;
            }


            foreach (DriveElement de in driveElements)
            {
                de.Initialize();
            }
            wheelsAmount = driveElements.Length;
            isSetup = true;
        }

        public bool IsGrounded()
        {
            return Physics.Raycast(groundCheck.position, -rig.transform.up, out RaycastHit hit, groundingRayLength);
        }

        private void FixedUpdate()
        {
            if (!isSetup) return;

            PhysicsCalculations();

            if (grounded)
            {
                StepUp();
                Friction();
                Brake();
            }
            
            
        }

        private void PhysicsCalculations()
        {
            carVelocity = rig.transform.InverseTransformDirection(rig.velocity); //local velocity of car

            float turnInput = carStats.TurnPower * playerInputs.Horizontal * Time.fixedDeltaTime * 1000;
            float speedInput = carStats.EnginePower * playerInputs.Vertical * Time.fixedDeltaTime * 1000;

            if (isLocalBraking)
                brakeInput = carStats.BrakePower * Time.fixedDeltaTime * 1000;

            speedValue = speedInput * carStats.SpeedCurve.Evaluate(Mathf.Abs(carVelocity.z) / 100);
            fricValue = carStats.FrictionPower * carStats.FrictionCurve.Evaluate(carVelocity.magnitude / 100);
            turnValue = turnInput * carStats.TurningCurve.Evaluate(carVelocity.magnitude / 100);

            if (grounded)
            {
                rig.angularDrag = carStats.Drag;
            }
            else
            {
                rig.drag = 0.1f;
                rig.centerOfMass = centerOfMass.localPosition;
                rig.angularDrag = 0.1f;
            }
        }
        private void ReadInputs()
        {

            currentSpeed = rig.velocity.magnitude * 4f;
            currentSpeedRatio = currentSpeed / carStats.MaxForwardSpeed;

            if (!playerInputs.Brake)
            {
                isLocalBraking = playerInputs.CombinedInput == 0;
            }
        }

        private void StepUp()
        {
            if (playerInputs.CombinedInput == 0) return;

            foreach (DriveElement de in driveElements)
            {
                Ray ray = new Ray(de.StepDetector.position, de.StepDetector.forward);
                if (Physics.Raycast(ray, out RaycastHit hit, carStats.StepUpRayLength))
                {
                    de.WheelRigidbody.AddForceAtPosition(carStats.StepUpForce * de.WheelRigidbody.transform.up + de.WheelRigidbody.transform.forward, 
                        de.WheelRigidbody.transform.position - de.WheelRigidbody.transform.up);
                    Debug.DrawRay(ray.origin, ray.direction * hit.distance, Color.green);
                }
                else
                {
                    Debug.DrawRay(ray.origin, ray.direction * carStats.StepUpRayLength, Color.red);
                }
            }
        }
        private void Acceleration()
        {
           
            if (playerInputs.Vertical > 0.1f)
            {
                foreach(DriveElement de in driveElements)
                {
                    if (!de.Drive) continue;
                    rig.AddForceAtPosition(rig.transform.forward * speedValue/wheelsAmount 
                        * carStats.EngineCurve.Evaluate(currentSpeed), de.ForceAtPosition.position);
                }
                
            }
            if (playerInputs.Vertical < -0.1f)
            {
                foreach (DriveElement de in driveElements)
                {
                    if (!de.Drive) continue;

                    rig.AddForceAtPosition(rig.transform.forward * speedValue / wheelsAmount
                        * carStats.EngineCurve.Evaluate(currentSpeed), de.ForceAtPosition.position);
                }
            }
        }


        private void Friction()
        {
            //Friction
            if (carVelocity.magnitude > 1)
            {
                terrainAngle = (-Vector3.Angle(rig.transform.up, Vector3.up) / 90f) + 1;
                foreach(DriveElement de in driveElements)
                {
                    rig.AddForceAtPosition(rig.transform.right * fricValue/wheelsAmount * terrainAngle
                        * 100f * -carVelocity.normalized.x, de.ForceAtPosition.position);
                }
                
            }
        }

        private void Brake()
        {
            if (carVelocity.z > 1f)
            {
                rig.AddForceAtPosition(rig.transform.forward * -brakeInput, groundCheck.position);
            }
            if (carVelocity.z < -1f)
            {
                rig.AddForceAtPosition(rig.transform.forward * brakeInput, groundCheck.position);
            }


            if (carVelocity.magnitude < 1)
            {
                rig.drag = 5f;
            }
            else
            {
                rig.drag = 0.1f;
            }
        }
        private void VisualEffects()
        {
            foreach(DriveElement de in driveElements)
            {
                float dir = playerInputs.Vertical > 0 ? -1 : 1f;
                Vector3 rotateAroundAxis = -de.VisualWheel.right;

                de.MeshTransform.RotateAround(de.VisualWheel.position, rotateAroundAxis, dir * currentSpeedRatio * 10f);
                if (de.CanSteer)
                {
                    de.VisualWheel.localRotation = Quaternion.RotateTowards(de.VisualWheel.localRotation,
                    Quaternion.Euler(de.VisualWheel.localRotation.eulerAngles.x, ClampAngle(carStats.TurnAngle * playerInputs.Horizontal, -carStats.TurnAngle, carStats.TurnAngle), de.VisualWheel.localRotation.eulerAngles.z),
                    Time.deltaTime*180f);
                }

               
            }
            speedometer.text = currentSpeed.ToString("F0");
        }


        
        public void TurningLogic()
        {
            if (!grounded || !isSetup) return;
            //turning
            if (carVelocity.z > 0.2f)
            {
                rig.AddTorque(rig.transform.up * turnValue);
            }
            else if (playerInputs.Vertical > 0.2f)
            {
                rig.AddTorque(rig.transform.up * turnValue);
            }
            if (carVelocity.z < -0.2f && playerInputs.Vertical < -0.2f)
            {
                rig.AddTorque(rig.transform.up * -turnValue);
            }
        }

        public void MovementLogic()
        {
            if (!grounded || !isSetup) return;

            Acceleration();
        }


        public float ClampAngle(float angle, float min, float max)
        {
            angle = Mathf.Repeat(angle, 360);
            min = Mathf.Repeat(min, 360);
            max = Mathf.Repeat(max, 360);
            bool inverse = false;
            var tmin = min;
            var tangle = angle;
            if (min > 180)
            {
                inverse = !inverse;
                tmin -= 180;
            }
            if (angle > 180)
            {
                inverse = !inverse;
                tangle -= 180;
            }
            var result = !inverse ? tangle > tmin : tangle < tmin;
            if (!result)
                angle = min;

            inverse = false;
            tangle = angle;
            var tmax = max;
            if (angle > 180)
            {
                inverse = !inverse;
                tangle -= 180;
            }
            if (max > 180)
            {
                inverse = !inverse;
                tmax -= 180;
            }

            result = !inverse ? tangle < tmax : tangle > tmax;
            if (!result)
                angle = max;
            return angle;
        }

    }

}

