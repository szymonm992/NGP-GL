using Frontend.Scripts.Models;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Frontend.Scripts.Components
{
    public class UnderWheel : MonoBehaviour
    {
        #region blinkachu
        /*
        [SerializeField] private Rigidbody rig;

        public float wheelRadius;

        public float restLength;
        public float springTravel;
        public float spring;
        public float damper;

        private float minLength;
        private float maxLength;
        private float lastLength;
        private float springLength;
        private float springVelocity;
        private float springForce;
        private float damperForce;

        private float speed;

        private Vector3 suspensionForce;

        private bool isColliding;
        public bool IsColliding => isColliding;

        private void Start()
        {
           
            minLength = restLength - springTravel;
            maxLength = restLength + springTravel;

            springLength = maxLength;
        }

   
        private void FixedUpdate()
        {
            speed = rig.velocity.magnitude * 4f;

            isColliding = Physics.Raycast(transform.position, -transform.up, out RaycastHit hit, maxLength + wheelRadius);

            if(isColliding)
            {
                lastLength = springLength;
                springLength = hit.distance - wheelRadius;

               
                springLength = Mathf.Clamp(springLength, minLength, maxLength);

                springVelocity = (lastLength - springLength) / Time.fixedDeltaTime;
                springForce = spring * (restLength - springLength);
                damperForce = damper * springVelocity;

                suspensionForce = (springForce + damperForce) * transform.up;

                var fx = Input.GetAxis("Vertical") * springForce;

                rig.AddForceAtPosition(suspensionForce  + (fx * transform.forward), hit.point);

            }

        }*/
        #endregion

        private Rigidbody rig;

        [SerializeField] private float wheelRadius = 0.35f;
        [SerializeField] private float mass = 20f;
        [SerializeField] private float suspensionTravel = 0.3f;
        [SerializeField] private float spring = 20000f;
        [SerializeField] private float damper = 2000f;
        [SerializeField] private float rollingResistance = 0.01f;

        [SerializeField] private UnderFrictionCurve ForwardFriction = new UnderFrictionCurve()
        {
            ExtremumSlip = 0.4f,
            ExtremumValue = 1f,
            AsymptoteSlip = 0.8f,
            AsymptoteValue = 0.5f,
            Stiffness = 1f
        };

        private float motorTorque = 0f;
        private float brakeTorque = 0f;
        private float steerAngle = 0f;

        private bool isGrounded = false;

        public float MotorTorque
        {
            get { return this.motorTorque; }
            set { this.motorTorque = value; }
        }

        public float BrakeTorque
        {
            get { return this.brakeTorque; }
            set { this.brakeTorque = Mathf.Abs(value); }
        }

        #region Telemetry/readonly
        private float angularVelocity = 0f;
        private float tireRotation = 0f;

        private float differentialSlipRatio = 0f;
        private float differentialTanOfSlipAngle = 0f;
        private float slipAngle = 0f;
        private float previousSuspensionDistance = 0f;
        private float normalForce = 0f;

        private Vector3 finalForce;
        private Vector3 tirePosition;

        private const float RelaxationLengthLongitudal = 0.103f;
        private const float RelaxationLengthLateral = 0.303f;


        #endregion

        public void GetWorldPosition(out Vector3 position, out Quaternion rotation)
        {
            var localTireRotation = Quaternion.Euler(tireRotation * Mathf.Rad2Deg, 0, 0);
            position = GetTirePosition();
            rotation = transform.rotation * localTireRotation;
        }

        private void FixedUpdate()
        {
            Vector3 newPosition = GetTirePosition();
            Vector3 newVelocity = (newPosition - tirePosition) / Time.fixedDeltaTime;
            tirePosition = newPosition;
            normalForce = GetSuspensionForce(tirePosition) + mass * Mathf.Abs(Physics.gravity.y);

            float longitudalSpeed = Vector3.Dot(newVelocity, transform.forward);
            float lateralSpeed = Vector3.Dot(newVelocity, -transform.right);

            float longitudalForce = GetLongitudalForce(normalForce, longitudalSpeed);

            finalForce = normalForce * rig.transform.up + rig.transform.forward * longitudalForce; 

            if (isGrounded)
                rig.AddForceAtPosition(finalForce, tirePosition);
        }

        private void Start()
        {
            previousSuspensionDistance = suspensionTravel;
            tirePosition = GetTirePosition();
        }

        private void OnValidate()
        {
            if(!rig)
            {
                rig = transform.GetComponentInParent<Rigidbody>();
            }
        }

        private float GetSuspensionForce(Vector3 localTirePosition)
        {
            float distance = Vector3.Distance(transform.position - rig.transform.up * suspensionTravel, localTirePosition);
            float springForce = spring * distance;
            float damperForce = damper * ((distance - previousSuspensionDistance) / Time.fixedDeltaTime);
            previousSuspensionDistance = distance;
            return springForce + damperForce;
        }

        private float GetLongitudalForce(float localNormalForce, float localLongitudalSpeed)
        {
            float delta = CalculateSlipDelta(differentialSlipRatio, localLongitudalSpeed);
            differentialSlipRatio += delta * Time.fixedDeltaTime;
            return Mathf.Sign(DampenForLowSpeeds(differentialSlipRatio, delta, localLongitudalSpeed, 0.02f))
                * localNormalForce * ForwardFriction.Evaluate(differentialSlipRatio);
        }

        private float CalculateSlipDelta(float localDifferentialSleepRatio, float localLongitudalSpeed)
        {
            float longitudalAngularSpeed = angularVelocity * wheelRadius;
            float slipDelta = (longitudalAngularSpeed - localLongitudalSpeed) - Mathf.Abs(localLongitudalSpeed) * localDifferentialSleepRatio;
            return slipDelta / RelaxationLengthLongitudal;
        }

        private Vector3 GetTirePosition()
        {
            isGrounded = Physics.Raycast(new Ray(transform.position, -rig.transform.up), out RaycastHit hit, wheelRadius + suspensionTravel);
            if(isGrounded)
            {
                return hit.point + (rig.transform.up * wheelRadius);
            }
            else
            {
                return transform.position - (rig.transform.up * suspensionTravel);
            }
        }

        private float DampenForLowSpeeds(float value, float delta, float speed, float tau)
        {
            if(speed > 0.15f)
            {
                tau = 0f;
            }
            return value + tau * delta;
        }

        private void OnDrawGizmos()
        {
            if(rig!=null)
            {
                Gizmos.color = Color.yellow;
                Handles.color = isGrounded ? Color.green : Color.red;

                GetWorldPosition(out Vector3 position, out Quaternion rotation);

                Handles.DrawWireDisc(position, transform.right, wheelRadius);
               
                Gizmos.DrawLine(transform.position, transform.position - (rig.transform.up * suspensionTravel));

                var force = (finalForce - normalForce * transform.up) / 1000f;
                Gizmos.DrawLine(transform.position, transform.position + force);

                Gizmos.color = isGrounded ? Color.green : Color.red;
                Gizmos.DrawSphere(position, .05f);
            }
        }
    }



}
