using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Frontend.Scripts
{
    public class TestWheel : MonoBehaviour
    {
        [SerializeField] private float restLength;
        [SerializeField] private float springTravel;
        [SerializeField] private float springStiffness;
        [SerializeField] private float damperStiffness;
        [SerializeField] private float wheelRadius;
        [SerializeField] private float tireMass = 20f;
        [Range(0,1f)]
        [SerializeField] private float tireGripFactor = 1f;

        public bool isLeft = false;
        public bool canSteer = false;
        public float steerAngle = 0f;
        private float wheelAngle = 0f;

        private float minLength;
        private float maxLength;
        private float lastLength;
        private float springLength;
        private float springVelocity;
        private float springForce;
        private float damperForce;

        private Vector3 suspensionForce;
        private Vector3 wheelWorldPosition;
        private Rigidbody rig;
        private bool isGrounded = false;
        private RaycastHit hit;
        private float debugLength;

        private Vector3 lastPosition;
        private Vector3 wheelVelocityLocal;
        private float Fx, Fy;

        private void OnValidate()
        {
            RefreshParams();
        }

        private void Start()
        {
            rig = transform.root.GetComponent<Rigidbody>();
            OnValidate();
        }

        private void Update()
        {
            wheelAngle = Mathf.Lerp(wheelAngle, steerAngle, Time.deltaTime * 8f);
            transform.localRotation = Quaternion.Euler(transform.localRotation.x, 
                transform.localRotation.y + wheelAngle,
                transform.localRotation.z);
        }

        private void RefreshParams()
        {
            minLength = restLength - springTravel;
            maxLength = restLength + springTravel;
            springLength = minLength+maxLength/2;

            debugLength  = (springLength + wheelRadius);
            wheelWorldPosition = transform.position - transform.up * debugLength;
        }

        private void FixedUpdate()
        {
            isGrounded = Physics.SphereCast(transform.position,wheelRadius, -transform.up, out hit, maxLength + wheelRadius);
            if (isGrounded)
            {
                ApplySpringForces();
                ApplyFrictionForces();
            }
            else
            {
                debugLength = (maxLength + wheelRadius);
                wheelWorldPosition = transform.position - transform.up * debugLength;
            }
        }   

        private void ApplySpringForces()
        {
            lastLength = springLength;
            springLength = hit.distance - wheelRadius;
            springLength = Mathf.Clamp(springLength, minLength, maxLength);
            springVelocity = (lastLength - springLength) / Time.fixedDeltaTime;
            springForce = springStiffness * (restLength - springLength);
            damperForce = damperStiffness * springVelocity;

            suspensionForce = (springForce + damperForce) * transform.up;

            wheelVelocityLocal = transform.InverseTransformDirection(rig.GetPointVelocity(hit.point));

           
            Fx = Input.GetAxis("Vertical") * springForce/2f;
            Fy = wheelVelocityLocal.x * springForce;

            rig.AddForceAtPosition(suspensionForce + (Fx*transform.forward) + (Fy * -transform.right) , hit.point);

            debugLength = (springLength + wheelRadius);
            wheelWorldPosition = hit.point + transform.up * wheelRadius;
        }

        private void ApplyFrictionForces()
        {
            Vector3 steeringDir = transform.right;
            Vector3 tireVel = rig.GetPointVelocity(transform.position);

            float steeringVel = Vector3.Dot(steeringDir, tireVel);
            float desiredVelChange = -steeringVel * tireGripFactor;
            float desiredAccel = desiredVelChange / Time.fixedDeltaTime;

            rig.AddForceAtPosition(steeringDir * tireMass * desiredAccel, hit.point);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(transform.position, 0.04f);
            Gizmos.DrawSphere(wheelWorldPosition, 0.04f);

            Gizmos.color = isGrounded ? Color.green : Color.red;

            Gizmos.DrawWireSphere(wheelWorldPosition, wheelRadius);
            Gizmos.DrawLine(transform.position, transform.position - transform.up * debugLength);
            Gizmos.DrawSphere(hit.point, 0.04f);
        }

        /*
        [SerializeField] private Transform[] wheelTransforms;
        [SerializeField] private Rigidbody rig;

        [SerializeField] private float suspensionRestDist = 0.3f;
        [SerializeField] private float springStrength = 200000f;
        [SerializeField] private float springDamper = 2000f;
        [SerializeField] private float wheelDiameter = 0.6f;
        [SerializeField] private float tireGripFactor = 1f;
        [SerializeField] private float tireMass = 250f;
        [SerializeField] private float carTopSpeed = 20f;
        private float inputY, inputX;

        [SerializeField] private AnimationCurve powerCurve;

        private void Update()
        {
            inputY = Input.GetAxis("Vertical") * carTopSpeed * 10f;    
        }

        private void FixedUpdate()
        {
            foreach(var wheel in wheelTransforms)
            {
                bool rayDidHit = CheckRayHit(wheel, out float tireDistance);
                if(rayDidHit)
                {
                    #region SPRING
                    Vector3 springDir = wheel.up;

                    Vector3 tireVelocity = rig.GetPointVelocity(wheel.position);

                    float offset = suspensionRestDist - tireDistance;

                    float vel = Vector3.Dot(springDir, tireVelocity);

                    float force = (offset * springStrength) - (vel * springDamper);

                    rig.AddForceAtPosition(springDir * force, wheel.position);
                    #endregion
                    Driving(wheel, inputY);
                    Steering(wheel);
                    
                }
            }
        }

        private void Driving(Transform wheel,float input)
        {
            Vector3 accelDir = wheel.forward;
            if(Mathf.Abs(input)>0f)
            {
                float carSpeed = Vector3.Dot(transform.forward, rig.velocity);

                float normalizedSpeed = Mathf.Clamp01(Mathf.Abs(carSpeed) / carTopSpeed);

                float availableTorque = powerCurve.Evaluate(normalizedSpeed) * input;
                rig.AddForceAtPosition(accelDir * availableTorque, wheel.position);
            }
        }

        private void Steering( Transform wheel)
        {
            Vector3 steeringDir = wheel.right;

            Vector3 tireVel = rig.GetPointVelocity(wheel.position);

            float steeringVel = Vector3.Dot(steeringDir, tireVel);

            float desiredVelChange = -steeringVel * tireGripFactor;

            float desiredAccel = desiredVelChange / Time.fixedDeltaTime;

            rig.AddForceAtPosition(steeringDir * tireMass * desiredAccel, wheel.position);
        }
        private bool CheckRayHit(Transform wheel, out float distance)
        {
            distance = 0;
            Ray ray = new Ray(wheel.position + wheel.up * wheelDiameter/2f, -wheel.up);
            
            if (Physics.Raycast(ray.origin, ray.direction, out RaycastHit hit, wheelDiameter))
            {
                distance = hit.distance/2f;
                Debug.DrawRay(ray.origin, ray.direction * hit.distance, Color.green);
                return true;
            }
            Debug.DrawRay(ray.origin, ray.direction* wheelDiameter, Color.red);
            return false;
        }*/
    }
}
