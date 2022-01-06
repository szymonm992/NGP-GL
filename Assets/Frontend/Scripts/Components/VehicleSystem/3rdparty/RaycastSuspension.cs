using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Frontend.Scripts
{
    public class RaycastSuspension : MonoBehaviour
    {

        #region REGION - Unity Editor Inspector Assignable Fields

        public Rigidbody rigidBody;
        public Transform steeringTransform;
        public Transform suspensionTransform;
        public Transform wheelTransform;

        public float wheelRadius = 0.5f;
        public float wheelMass = 1f;//used to simulate wheel rotational inertia for brakes and friction purposes
        public float suspensionLength = 0.5f;
        public float target = 0;
        public float spring = 1000;
        public float damper = 1500;
        public float maxMotorTorque = 0;
        public float rpmLimit = 600f;
        public float maxBrakeTorque = 0;
        public float maxSteerAngle = 0;
        public float throttleResponse = 2;
        public float steeringResponse = 2;
        public float brakeResponse = 2;

        public float springCurve = 0f;
        public float forwardFrictionCoefficient = 1f;
        public float sideFrictionCoefficient = 1f;
        public float surfaceFrictionCoefficient = 1f;


        public bool debug = false;

        #endregion ENDREGION - Unity Editor Inspector Assignable Fields


        #region REGION - Unity Editor Display-Only Variables

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



        #endregion ENDREGION - Unity Editor Display Variables

        private RWheelCollider wheelCollider;

        private float currentMotorTorque;
        private float currentSteer;
        private float currentBrakeTorque;

        private GameObject bumpStopCollider;

        public void Start()
        {
            wheelCollider = gameObject.AddComponent<RWheelCollider>();
            wheelCollider.Rigidbody = this.rigidBody;
            bumpStopCollider = new GameObject("BSC-" + wheelCollider.name);
            SphereCollider sc = bumpStopCollider.AddComponent<SphereCollider>();
            PhysicMaterial mat = new PhysicMaterial("TEST");
            mat.bounciness = 0.0f;
            mat.dynamicFriction = 0;
            mat.staticFriction = 0;
            sc.material = mat;
            OnValidate();
        }

        private void InputHandling()
        {
            float left = Input.GetAxis("Horizontal") < 0 ? -1 : 0;
            float right = Input.GetAxis("Horizontal") > 0 ? 1 : 0;
            float fwd = Input.GetAxis("Vertical") > 0 ? 1 : 0;
            float rev = Input.GetAxis("Vertical") < 0 ? -1 : 0;

            float brakeInput = 0;
            if (Input.GetAxis("Vertical") == 0)
            {
                brakeInput = 1;
            }
            if(Input.GetKey(KeyCode.Space))
            {
                brakeInput = 2;
            }
            float forwardInput = fwd + rev;
            float turnInput = left + right;

            float rpm = wheelCollider.rpm;

            if (rpm >= rpmLimit && forwardInput > 0) { forwardInput = 0; }
            else if (rpm <= -rpmLimit && forwardInput < 0) { forwardInput = 0; }
            currentMotorTorque = Mathf.Lerp(currentMotorTorque, forwardInput * maxMotorTorque, throttleResponse * Time.fixedDeltaTime);


            if (forwardInput == 0 && Mathf.Abs(currentMotorTorque) < 0.25f) { currentMotorTorque = 0f; }
            currentSteer = Mathf.Lerp(currentSteer, turnInput * maxSteerAngle, steeringResponse * Time.fixedDeltaTime);

            if (turnInput == 0 && Mathf.Abs(currentSteer) < 0.25f) { currentSteer = 0f; }
            currentBrakeTorque = Mathf.Lerp(currentBrakeTorque, brakeInput * maxBrakeTorque, brakeResponse * Time.fixedDeltaTime);

            if (brakeInput == 0 && currentBrakeTorque < 0.25f) { currentBrakeTorque = 0f; }
        }

        public void FixedUpdate()
        {
            Vector3 targetPos = transform.position;
            Vector3 pos = bumpStopCollider.transform.position;
            Vector3 p = Vector3.Lerp(pos, targetPos, Time.fixedDeltaTime);
            bumpStopCollider.transform.position = p;

            InputHandling();
            wheelCollider.motorTorque = currentMotorTorque;
            wheelCollider.steeringAngle = currentSteer;
            wheelCollider.brakeTorque = currentBrakeTorque;
            wheelCollider.UpdateWheel();
            if (steeringTransform != null)
            {
                steeringTransform.localRotation = Quaternion.AngleAxis(currentSteer, steeringTransform.up);
            }
            if (suspensionTransform != null)
            {
                suspensionTransform.position = gameObject.transform.position - (suspensionLength - wheelCollider.compressionDistance) * gameObject.transform.up;
            }
            if (wheelTransform != null)
            {
                wheelTransform.Rotate(wheelTransform.right, wheelCollider.perFrameRotation, Space.World);
            }
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
            if (debug)
            {
                Debug.Log("spring: " + fSpring + " damper: " + fDamp);
            }
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

                SphereCollider sc = bumpStopCollider.GetComponent<SphereCollider>();
                bumpStopCollider.layer = 26;
                sc.radius = wheelRadius;
                bumpStopCollider.transform.parent = gameObject.transform;
                bumpStopCollider.transform.localPosition = Vector3.zero;
            }
        }

        void OnDrawGizmosSelected()
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


