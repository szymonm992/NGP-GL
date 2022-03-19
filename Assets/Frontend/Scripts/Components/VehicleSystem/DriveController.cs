using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
using Frontend.Scripts.Models.VehicleSystem;
using Frontend.Scripts.Enums;

namespace Frontend.Scripts
{
    [System.Serializable]
    public struct DriveElement
    {
        
        public Transform forceAtPos;
        public Transform detetion;

    }
    public class DriveController : MonoBehaviour
    {

        [Inject] public readonly IVehicleStats tankStats;
        [Inject] public readonly IPlayerInput playerInputs;


        [Header("Suspension")]
        public Transform groundCheck;
        public Transform fricAt;
        public Transform CentreOfMass;
        public Transform ForceAtPos;
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
        [HideInInspector]
        public bool grounded;

        [Header("Curves")]
        public AnimationCurve frictionCurve;
        public AnimationCurve speedCurve;
        public AnimationCurve turnCurve;
        public AnimationCurve engineCurve;


        [Inject(Id = "mainRig")] private readonly Rigidbody rig;

        private float speedValue, fricValue, turnValue, brakeInput;
        [HideInInspector]
        public Vector3 carVelocity;
        [HideInInspector]
        public RaycastHit hit;

        [Header("Other Settings")]
    
        public bool airDrag;
        public bool AirVehicle = false;
        public float UpForce;
        private float frictionAngle;


        #region LOCAL CONTROL VARIABLES
        private Vector2 inputs;
        private float combinedInput = 0f;
        private bool isBraking = false;
        private float currentSpeed = 0f;
        private float finalPower = 0f;
        #endregion

        private void Update()
        {
            inputs = new Vector2(playerInputs.Horizontal, playerInputs.Vertical);
            combinedInput = Mathf.Abs(playerInputs.Horizontal) + Mathf.Abs(playerInputs.Vertical);
            currentSpeed = rig.velocity.magnitude * 4f;
            isBraking = playerInputs.Brake;
            if(!isBraking)
            {
                isBraking = combinedInput == 0;
            }
        }


        private void Awake()
        {
            grounded = false;
            rig.centerOfMass = CentreOfMass.localPosition;
        }

        void FixedUpdate()
        {
            carVelocity = rig.transform.InverseTransformDirection(rig.velocity); //local velocity of car

            //inputs
            float turnInput = turn * inputs.x * Time.fixedDeltaTime * 1000;
            float speedInput = speed * inputs.y * Time.fixedDeltaTime * 1000;
            if (isBraking)
                brakeInput = brake * Time.fixedDeltaTime * 1000;

            //helping veriables
            speedValue = speedInput * speedCurve.Evaluate(Mathf.Abs(carVelocity.z) / 100);
            fricValue = friction * frictionCurve.Evaluate(carVelocity.magnitude / 100);
            turnValue = turnInput * turnCurve.Evaluate(carVelocity.magnitude / 100);

            //grounded check
            if (Physics.Raycast(groundCheck.position, -rig.transform.up, out hit, maxRayLength) && AirVehicle == false)
            {

                StepUp();


                accelarationLogic();
                turningLogic();
                frictionLogic();
                brakeLogic();
               
                rig.angularDrag = dragAmount;

                Debug.DrawLine(groundCheck.position, hit.point, Color.green);
                grounded = true;

                rig.centerOfMass = Vector3.zero;
            }
            else if (!Physics.Raycast(groundCheck.position, -rig.transform.up, out hit, maxRayLength) && AirVehicle == false)
            {
                grounded = false;
                rig.drag = 0.1f;
                rig.centerOfMass = CentreOfMass.localPosition;
                if (!airDrag)
                {
                    rig.angularDrag = 0.1f;
                }
            }

            if (AirVehicle == true)
            {
                AirController();
            }

           
        }

        private void StepUp()
        {
            if (combinedInput == 0) return;

            foreach (DriveElement de in driveElements)
            {
                Ray ray = new Ray(de.detetion.position, de.detetion.forward);
                if (Physics.Raycast(ray, out RaycastHit hit, stepDetectiongRange))
                {
                    de.forceAtPos.parent.GetComponent<Rigidbody>().AddForceAtPosition(stepUp * de.forceAtPos.parent.up + de.forceAtPos.parent.forward, 
                        de.forceAtPos.parent.position - de.forceAtPos.parent.up);
                    Debug.DrawRay(ray.origin, ray.direction * hit.distance, Color.green);
                }
                else
                {
                    Debug.DrawRay(ray.origin, ray.direction * stepDetectiongRange, Color.red);
                }
            }
        }
        public void accelarationLogic()
        {
            //speed control
            if (inputs.y > 0.1f)
            {
                foreach(DriveElement de in driveElements)
                {
                    rig.AddForceAtPosition(rig.transform.forward * speedValue/driveElements.Length * engineCurve.Evaluate(currentSpeed), de.forceAtPos.position);
                }
                
            }
            if (inputs.y < -0.1f)
            {
                foreach (DriveElement de in driveElements)
                {
                    rig.AddForceAtPosition(rig.transform.forward * speedValue/2 / driveElements.Length * engineCurve.Evaluate(currentSpeed), de.forceAtPos.position);
                }
            }
        }

        public void turningLogic()
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

        public void frictionLogic()
        {
            //Friction
            if (carVelocity.magnitude > 1)
            {
                frictionAngle = (-Vector3.Angle(rig.transform.up, Vector3.up) / 90f) + 1;
                foreach(DriveElement de in driveElements)
                {
                    rig.AddForceAtPosition(rig.transform.right * fricValue/driveElements.Length * frictionAngle * 100 * -carVelocity.normalized.x, de.forceAtPos.position);
                }
                
            }
        }

        public void brakeLogic()
        {
            //brake
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

        public void AirController()
        {
            rig.useGravity = false;
            var forwardDir = Vector3.ProjectOnPlane(rig.transform.forward, Vector3.up);


            float upForceValue = (-Physics.gravity.y / Time.deltaTime) + UpForce;
            rig.AddForce(rig.transform.up * upForceValue * Time.deltaTime);

            //speed control
            if (inputs.y > 0.1f)
            {
                rig.AddForceAtPosition(forwardDir * speedValue, groundCheck.position);
            }
            if (inputs.y < -0.1f)
            {
                rig.AddForceAtPosition(forwardDir * speedValue / 2, groundCheck.position);
            }

            rig.AddTorque(Vector3.up * turnValue);

            //friction(drag) 
            if (carVelocity.magnitude > 1)
            {
                float frictionAngle = (-Vector3.Angle(rig.transform.up, Vector3.up) / 90f) + 1;
                foreach (DriveElement de in driveElements)
                {
                    rig.AddForceAtPosition(Vector3.ProjectOnPlane(rig.transform.right, Vector3.up) * fricValue/driveElements.Length * frictionAngle * 100 * -carVelocity.normalized.x, de.forceAtPos.position);
                }
            }

            //brake
            if (carVelocity.z > 1f)
            {
                rig.AddForceAtPosition(Vector3.ProjectOnPlane(rig.transform.forward, Vector3.up) * -brakeInput, groundCheck.position);
            }
            if (carVelocity.z < -1f)
            {
                rig.AddForceAtPosition(Vector3.ProjectOnPlane(rig.transform.forward, Vector3.up) * brakeInput, groundCheck.position);
            }
            if (carVelocity.magnitude < 1)
            {
                rig.drag = 5f;
            }
            else
            {
                rig.drag = 1f;
            }


        }


    }
    
}

