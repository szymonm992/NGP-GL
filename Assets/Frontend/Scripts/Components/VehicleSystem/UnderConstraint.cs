using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Frontend.Scripts
{
    public class UnderConstraint : MonoBehaviour
    {
        public Rigidbody rigidBody;

        public float wheelRadius = 0.4f;
        public float wheelMass = 50f;
        public float suspensionLength = 0.25f;
        public float target = 0;
        public float spring = 30000;
        public float damper = 2500;
        public float maxMotorTorque = 0;
        public float rpmLimit = 1000f;
        public float maxBrakeTorque = 3000;
        public float throttleResponse = 5;
        public float brakeResponse = 10;

        public float springCurve = 0f;
        public float forwardFrictionCoefficient = 1f;
        public float sideFrictionCoefficient = 2f;
        public float surfaceFrictionCoefficient = 1f;

        [Header("Telemetry")]
        public Vector3 localVelocity;
        public Vector3 localAcceleration;
        public float rpm;
        public float sLong;
        public float sLat;
        public float fSpring;
        public float fDamp;
        public float fLong;
        public float fLat;
        public float comp;

        private RWheelCollider wheelCollider;

        private float currentMotorTorque;
        private float currentBrakeTorque;


        public void Start()
        {
            wheelCollider = gameObject.AddComponent<RWheelCollider>();
            wheelCollider.Rigidbody = this.rigidBody;
            OnValidate();
        }

        private void InputHandling()
        {
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");

            float brakeInput = 0;
            if (Input.GetAxis("Vertical") == 0)
            {
                brakeInput = 1;
            }
            if (Input.GetKey(KeyCode.Space))
            {
                brakeInput = 2;
            }

            float rpm = wheelCollider.rpm;

            if (rpm >= rpmLimit && vertical > 0) 
            { 
                vertical = 0; 
            }
            else if (rpm <= -rpmLimit && vertical < 0)
            { 
                vertical = 0; 
            }
            currentMotorTorque = Mathf.Lerp(currentMotorTorque, vertical * maxMotorTorque, throttleResponse * Time.fixedDeltaTime);
            
            if (brakeInput == 0 && currentBrakeTorque < 0.25f)
            {
                currentBrakeTorque = 0f;
            }
            currentBrakeTorque = Mathf.Lerp(currentBrakeTorque, brakeInput * maxBrakeTorque, brakeResponse * Time.fixedDeltaTime);
        }

        public void FixedUpdate()
        {


            InputHandling();
            wheelCollider.motorTorque = currentMotorTorque;
            wheelCollider.brakeTorque = currentBrakeTorque;
            wheelCollider.UpdateWheel();

            Vector3 prevVel = localVelocity;
            localVelocity = wheelCollider.wheelLocalVelocity;
            localAcceleration = (prevVel - localVelocity) / Time.fixedDeltaTime;
            fSpring = wheelCollider.springForce;
            fDamp = wheelCollider.dampForce;
            rpm = wheelCollider.rpm;
            sLong = wheelCollider.longitudinalSlip;
            sLat = wheelCollider.lateralSlip;
            fLong = wheelCollider.longitudinalForce;
            fLat = wheelCollider.lateralForce;
            comp = wheelCollider.compressionDistance;
        }

        public void OnValidate()
        {
            if (wheelCollider != null)
            {
                wheelCollider.radius = wheelRadius;
                wheelCollider.mass = wheelMass;
                wheelCollider.length = suspensionLength;
                wheelCollider.spring = spring;
                wheelCollider.damper = damper;
                wheelCollider.motorTorque = maxMotorTorque;
                wheelCollider.brakeTorque = maxBrakeTorque;
                wheelCollider.forwardFrictionCoefficient = forwardFrictionCoefficient;
                wheelCollider.sideFrictionCoefficient = sideFrictionCoefficient;
                wheelCollider.surfaceFrictionCoefficient = surfaceFrictionCoefficient;

            }
        }

        void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(gameObject.transform.position, wheelRadius);
            Vector3 pos2 = gameObject.transform.position + -gameObject.transform.up * suspensionLength;
            if (wheelCollider != null) { pos2 += gameObject.transform.up * wheelCollider.compressionDistance; }
            Gizmos.DrawWireSphere(pos2, wheelRadius);
            Gizmos.DrawRay(gameObject.transform.position - gameObject.transform.up * wheelRadius, -gameObject.transform.up * suspensionLength);
        }

    }
}
