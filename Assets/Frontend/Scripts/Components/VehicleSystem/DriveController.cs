using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using Frontend.Scripts.Models.VehicleSystem;
using Frontend.Scripts.Enums;

namespace Frontend.Scripts
{
    [System.Serializable]
    public class DriveElement
    {
        public Rigidbody wheelRigidbody;

        private Transform forceAtPos;
        private Transform stepDetector;

        public Transform ForceAtPosition => forceAtPos;
        public Transform StepDetector => stepDetector;
        public void Initialize()
        {
            this.forceAtPos = wheelRigidbody.transform.Find("forceAtPosition");
            this.stepDetector = wheelRigidbody.transform.Find("stepDetector");
        }
    }
    public class DriveController : MonoBehaviour
    {

        [Inject] public readonly IVehicleStats tankStats;
        [Inject] public readonly IPlayerInput playerInputs;
        [SerializeField] private Text speedometer;

        [Header("Suspension")]
        public Transform groundCheck;
        public Transform CentreOfMass;
        public DriveElement[] driveElements;

        [Header("Car Stats")]
        public float speed = 200f;
        public float turn = 100f;
        public float brake = 150;
        public float friction = 70f;
        public float dragAmount = 4f;
        public float TurnAngle = 30f;
        public float stepUp = 2000f;
        public float stepDetectiongRange = .2f;
        public float maxRayLength = 0.8f, slerpTime = 0.2f;
        
        

        [Header("Curves")]
        public AnimationCurve frictionCurve;
        public AnimationCurve speedCurve;
        public AnimationCurve turnCurve;
        public AnimationCurve engineCurve;


        [Inject(Id = "mainRig")] private readonly Rigidbody rig;

        private float speedValue, fricValue, turnValue, brakeInput;
       
        #region LOCAL CONTROL VARIABLES
        private Vector2 inputs = Vector2.zero;
        private Vector3 carVelocity = Vector2.zero;
        private float combinedInput = 0f;
        private bool isBraking = false;
        private float currentSpeed = 0f;
        private int wheelsAmount=0;
        private float terrainAngle=0f;
        private bool grounded = false;
        #endregion

        private void Update()
        {
            ReadInputs();
            speedometer.text = currentSpeed.ToString("F0");
        }


        private void Awake()
        {
            rig.centerOfMass = CentreOfMass.localPosition;

            foreach(DriveElement de in driveElements)
            {
                de.Initialize();
            }
            wheelsAmount = driveElements.Length;
        }

        private void FixedUpdate()
        {
            carVelocity = rig.transform.InverseTransformDirection(rig.velocity); //local velocity of car

            float turnInput = turn * inputs.x * Time.fixedDeltaTime * 1000;
            float speedInput = speed * inputs.y * Time.fixedDeltaTime * 1000;

            if (isBraking)
                brakeInput = brake * Time.fixedDeltaTime * 1000;

            speedValue = speedInput * speedCurve.Evaluate(Mathf.Abs(carVelocity.z) / 100);
            fricValue = friction * frictionCurve.Evaluate(carVelocity.magnitude / 100);
            turnValue = turnInput * turnCurve.Evaluate(carVelocity.magnitude / 100);

            //grounded check
            if (Physics.Raycast(groundCheck.position, -rig.transform.up, out RaycastHit hit, maxRayLength))
            {

                StepUp();
                Acceleration();
                Turning();
                Friction();
                Brake();
               
                rig.angularDrag = dragAmount;

                Debug.DrawLine(groundCheck.position, hit.point, Color.green);
                grounded = true;

                rig.centerOfMass = Vector3.zero;
            }
            else if (!Physics.Raycast(groundCheck.position, -rig.transform.up, out hit, maxRayLength))
            {
                grounded = false;
                rig.drag = 0.1f;
                rig.centerOfMass = CentreOfMass.localPosition;
                rig.angularDrag = 0.1f;
            }

        }

        private void ReadInputs()
        {
            isBraking = playerInputs.Brake;

            inputs = new Vector2(playerInputs.Horizontal, playerInputs.Vertical);
            combinedInput = Mathf.Abs(playerInputs.Horizontal) + Mathf.Abs(playerInputs.Vertical);
            currentSpeed = rig.velocity.magnitude * 4f;

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
                if (Physics.Raycast(ray, out RaycastHit hit, stepDetectiongRange))
                {
                    de.wheelRigidbody.AddForceAtPosition(stepUp * de.wheelRigidbody.transform.up + de.wheelRigidbody.transform.forward, 
                        de.wheelRigidbody.transform.position - de.wheelRigidbody.transform.up);
                    Debug.DrawRay(ray.origin, ray.direction * hit.distance, Color.green);
                }
                else
                {
                    Debug.DrawRay(ray.origin, ray.direction * stepDetectiongRange, Color.red);
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
    }
    
}

