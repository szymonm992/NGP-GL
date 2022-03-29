using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using Frontend.Scripts.Models.VehicleSystem;
using Frontend.Scripts.Enums;

namespace Frontend.Scripts.Components
{
   
    
   
    public class DriveController : MonoBehaviour, IInitializable
    {

        [Inject] public readonly CarStats carStats;
        [Inject] public readonly IPlayerInput playerInputs;

        [Header("GUI")]
        [SerializeField] private Text speedometer;

        [Header("Detectors")]
        [SerializeField] private Transform groundCheck;
        [SerializeField] private Transform centerOfMass;

        [Space]
        [SerializeField] private DriveElement[] driveElements;
        [SerializeField] private float groundingRayLength = 0.8f;
        
        [Header("Curves")]
        [SerializeField] private AnimationCurve frictionCurve;
        [SerializeField] private AnimationCurve speedCurve;
        [SerializeField] private AnimationCurve turnCurve;
        [SerializeField] private AnimationCurve engineCurve;


        [Inject(Id = "mainRig")] private readonly Rigidbody rig;

        private float speedValue, fricValue, turnValue, brakeInput;
       
        #region LOCAL CONTROL VARIABLES
        private Vector2 inputs = Vector2.zero;
        private Vector3 carVelocity = Vector2.zero;
        
        private float currentSpeed = 0f;
        private float terrainAngle = 0f;
        private float combinedInput = 0f;
        private float currentSpeedRatio = 0f;

        private int wheelsAmount=0;

        private bool isBraking = false;
        private bool grounded = false;
        private bool isSetup=false;
        #endregion

        private void Update()
        {
            if (!isSetup) return;
            ReadInputs();
            speedometer.text = currentSpeed.ToString("F0");
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


        private void FixedUpdate()
        {
            if (!isSetup) return;
            carVelocity = rig.transform.InverseTransformDirection(rig.velocity); //local velocity of car

            float turnInput = carStats.TurnPower * inputs.x * Time.fixedDeltaTime * 1000;
            float speedInput = carStats.EnginePower * inputs.y * Time.fixedDeltaTime * 1000;

            if (isBraking)
                brakeInput = carStats.BrakePower * Time.fixedDeltaTime * 1000;

            speedValue = speedInput * speedCurve.Evaluate(Mathf.Abs(carVelocity.z) / 100);
            fricValue = carStats.FrictionPower * frictionCurve.Evaluate(carVelocity.magnitude / 100);
            turnValue = turnInput * turnCurve.Evaluate(carVelocity.magnitude / 100);

            if (Physics.Raycast(groundCheck.position, -rig.transform.up, out RaycastHit hit, groundingRayLength))
            {
                StepUp();
                Acceleration();
                Turning();
                Friction();
                Brake();
               
                rig.angularDrag = carStats.Drag;

                Debug.DrawLine(groundCheck.position, hit.point, Color.green);
                grounded = true;

                rig.centerOfMass = Vector3.zero;
            }
            else if (!Physics.Raycast(groundCheck.position, -rig.transform.up, out hit, groundingRayLength))
            {
                grounded = false;
                rig.drag = 0.1f;
                rig.centerOfMass = centerOfMass.localPosition;
                rig.angularDrag = 0.1f;
            }
            VisualEffects();
        }

        private void ReadInputs()
        {
            isBraking = playerInputs.Brake;

            inputs = new Vector2(playerInputs.Horizontal, playerInputs.Vertical);
            combinedInput = Mathf.Abs(playerInputs.Horizontal) + Mathf.Abs(playerInputs.Vertical);

            currentSpeed = rig.velocity.magnitude * 4f;
            currentSpeedRatio = currentSpeed / carStats.MaxForwardSpeed;

            if (!isBraking)
            {
                isBraking = combinedInput == 0;
            }
        }

        private void StepUp()
        {
            if (combinedInput == 0) return;

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
            //speed control
            if (inputs.y > 0.1f)
            {
                foreach(DriveElement de in driveElements)
                {
                    rig.AddForceAtPosition(rig.transform.forward * speedValue/wheelsAmount 
                        * engineCurve.Evaluate(currentSpeed), de.ForceAtPosition.position);
                }
                
            }
            if (inputs.y < -0.1f)
            {
                foreach (DriveElement de in driveElements)
                {
                    rig.AddForceAtPosition(rig.transform.forward * speedValue / wheelsAmount
                        * engineCurve.Evaluate(currentSpeed), de.ForceAtPosition.position);
                }
            }
        }

        private void Turning()
        {
            //turning
            if (carVelocity.z > 0.1f)
            {
                rig.AddTorque(rig.transform.up * turnValue);
            }
            else if (inputs.y > 0.1f)
            {
                rig.AddTorque(rig.transform.up * turnValue);
            }
            if (carVelocity.z < -0.1f && inputs.y < -0.1f)
            {
                rig.AddTorque(rig.transform.up * -turnValue);
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
            if (combinedInput == 0) return;
            foreach(DriveElement de in driveElements)
            {
                float dir = playerInputs.Vertical > 0 ? -1 : 1f;
                Vector3 rotateAroundAxis = -de.VisualWheel.right;

                if(de.CanSteer)
                {
                    de.VisualWheel.localRotation = Quaternion.RotateTowards(de.VisualWheel.localRotation,
                    Quaternion.Euler(de.VisualWheel.localRotation.eulerAngles.x, ClampAngle(carStats.TurnAngle * playerInputs.Horizontal, -carStats.TurnAngle + 5f, carStats.TurnAngle - 5f), de.VisualWheel.localRotation.eulerAngles.z),
                    Time.deltaTime*180f);
                }

                de.MeshTransform.RotateAround(de.VisualWheel.position, rotateAroundAxis, dir * currentSpeedRatio * 10f);
            }
        }


        public  float ClampAngle(float angle, float min, float max)
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

