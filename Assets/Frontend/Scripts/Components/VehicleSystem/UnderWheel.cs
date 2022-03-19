using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

using Frontend.Scripts.Enums;
using Frontend.Scripts.Models;

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

        [SerializeField] private UnderWheelDebug debugSettings = new UnderWheelDebug()
        {
            DrawGizmos = true,
            DrawOnDisable = false,
            DrawMode = UnderWheelDebugMode.All,
            DrawForce = true
        };

        [Header("Settings")]
        [SerializeField] [Range(0.1f, 1f)] private float suspensionTravel = 0.3f;
        [SerializeField] private float wheelRadius = 0.35f;
        [SerializeField] private float mass = 20f;
        [SerializeField] private float spring = 20000f;
        [SerializeField] private float damper = 2000f;
        [SerializeField] private float rollingResistance = 0.01f;
        [SerializeField] private float inertia = 3f;

        [SerializeField] private UnderFrictionCurve ForwardFriction = new UnderFrictionCurve()
        {
            ExtremumSlip = 0.4f,
            ExtremumValue = 1f,
            AsymptoteSlip = 0.8f,
            AsymptoteValue = 0.5f,
            Stiffness = 1f
        };

        [SerializeField]
        private UnderFrictionCurve SidewaysFriction = new UnderFrictionCurve()
        {
            ExtremumSlip = 15f,
            ExtremumValue = 1f,
            AsymptoteSlip = 30f,
            AsymptoteValue = 0.75f,
            Stiffness = 2f
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

        public float RPM
        {
            // multiplied by physics speed multiplier
            get { return Mathf.Abs(angularVelocity * 4f * 9.549296585f); }
        }

        #region Telemetry/readonly
        private float angularVelocity = 0f;
        private float tireRotation = 0f;

        private float differentialSlipRatio = 0f;
        private float differentialTanOfSlipAngle = 0f;
        private float slipAngle = 0f;
        private float previousSuspensionDistance = 0f;
        private float normalForce = 0f;
        private float compression = 0f;

        private Vector3 finalForce;
        private Vector3 tirePosition;

        private const float RelaxationLengthLongitudal = 0.103f;
        private const float RelaxationLengthLateral = 0.303f;


        #endregion

        public void GetWorldPosition(out Vector3 position, out Quaternion rotation)
        {
            var localTireRotation = Quaternion.Euler(tireRotation * Mathf.Rad2Deg, 0f, 0f);
            position = GetTirePosition();
            rotation = transform.rotation * localTireRotation;
        }

        public Vector3 GetTirePosition()
        {
            isGrounded = Physics.Raycast(new Ray(transform.position, -rig.transform.up), out RaycastHit hit, wheelRadius + suspensionTravel);
            if (isGrounded)
            {
                compression = hit.distance - wheelRadius;
                return hit.point + (rig.transform.up * wheelRadius);
            }
            else
            {
                compression = suspensionTravel;
                return transform.position - (rig.transform.up * suspensionTravel);
            }
        }

        private void FixedUpdate()
        {
           

            Vector3 newPosition = GetTirePosition();
            Vector3 newVelocity = (newPosition - tirePosition) / Time.fixedDeltaTime;
            tirePosition = newPosition;
            normalForce = GetSuspensionForce(tirePosition) + mass * Mathf.Abs(Physics.gravity.y);

            float longitudalSpeed = Vector3.Dot(newVelocity, transform.forward);
            float lateralSpeed = Vector3.Dot(newVelocity, -transform.right);

            float lateralForce = GetLateralForce(normalForce, lateralSpeed, longitudalSpeed);
            float longitudalForce = GetLongitudalForce(normalForce, longitudalSpeed);


            finalForce = normalForce * rig.transform.up 
                + rig.transform.forward * longitudalForce
                +rig.transform.right * lateralForce;
            if (!isGrounded) return;

            rig.AddForceAtPosition(finalForce, tirePosition);

            UpdateAngularVelocity(longitudalForce);
        }

        private void Start()
        {
            previousSuspensionDistance = suspensionTravel;
            compression = suspensionTravel;
            tirePosition = GetTirePosition();
        }

        private void OnValidate()
        {
            if(!rig)
            {
                rig = transform.GetComponentInParent<Rigidbody>();
            }
            if(compression == 0f)
            {
                compression = suspensionTravel;
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

        private float GetLateralForce(float normalForce, float lateralSpeed, float longitudalSpeed)
        {
            slipAngle = CalculatSlipAngle(longitudalSpeed, lateralSpeed);
            float coefficient = SidewaysFriction.Evaluate(slipAngle);
            coefficient *= Mathf.Sqrt(1f- Mathf.Pow(ForwardFriction.Evaluate(differentialSlipRatio) / ForwardFriction.ExtremumValue, 2f));
            return Mathf.Sign(slipAngle) * coefficient * normalForce;
        }
        private float CalculatSlipAngle(float longitudalSpeed, float lateralSpeed)
        {
            float delta = lateralSpeed - Mathf.Abs(longitudalSpeed) * differentialTanOfSlipAngle;
            delta /= RelaxationLengthLateral;
            differentialTanOfSlipAngle += delta * Time.fixedDeltaTime;
            return Mathf.Atan(DampenForLowSpeeds(differentialTanOfSlipAngle, delta, lateralSpeed, 0.1f)) * Mathf.Rad2Deg;   
        }
        private float CalculateSlipDelta(float localDifferentialSleepRatio, float localLongitudalSpeed)
        {
            float longitudalAngularSpeed = angularVelocity * wheelRadius;
            float slipDelta = (longitudalAngularSpeed - localLongitudalSpeed) - Mathf.Abs(localLongitudalSpeed) * localDifferentialSleepRatio;
            return slipDelta / RelaxationLengthLongitudal;
        }

        private void UpdateAngularVelocity(float longitudalForce)
        {
            float rollingResistanceForce = angularVelocity * wheelRadius * rollingResistance * normalForce;
            float torqueFromTireForce = (rollingResistanceForce + longitudalForce) * wheelRadius;
            float angularAcceleration = (motorTorque - Mathf.Sign(angularVelocity) * brakeTorque - torqueFromTireForce) / inertia;
            if(WillBrakesLock(angularAcceleration, torqueFromTireForce))
            {
                angularVelocity = 0f;
                return;
            }
            angularVelocity += angularAcceleration * Time.fixedDeltaTime;
            tireRotation = (tireRotation + angularVelocity * Time.fixedDeltaTime) % (2f * Mathf.PI);
        }

        private bool WillBrakesLock(float angularAcceleration, float torqueFromTireForce)
        {
            if(brakeTorque < Mathf.Abs(motorTorque - torqueFromTireForce))
            {
                return false;
            }
            if(brakeTorque > 0f && Mathf.Sign(angularVelocity) != Mathf.Sign(angularVelocity + angularAcceleration * Time.fixedDeltaTime))
            {
                return true;
            }
            return false;
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
            
            bool drawCurrently = (debugSettings.DrawGizmos) && (debugSettings.DrawMode == UnderWheelDebugMode.All)
                || (debugSettings.DrawMode == UnderWheelDebugMode.EditorOnly && !Application.isPlaying) 
                || (debugSettings.DrawMode == UnderWheelDebugMode.PlaymodeOnly && Application.isPlaying);

            if(drawCurrently && (debugSettings.DrawOnDisable && !this.enabled) || (this.enabled))
            {
                if (rig != null)
                {
                    Handles.color = isGrounded ? Color.green : Color.red;

                    GetWorldPosition(out Vector3 position, out Quaternion rotation);

                    Handles.DrawWireDisc(position, transform.right, wheelRadius);

                    Handles.DrawDottedLine(transform.position, transform.position - (rig.transform.up * compression), 1.1f);

                    //Vector3 upper = position + (rig.transform.up * suspensionTravel);
                    //upper border visualisation
                    //Handles.DrawWireArc(upper, transform.right, -transform.forward, 180, wheelRadius);
             
                  
                    if (debugSettings.DrawForce)
                    {
                        var force = (finalForce - normalForce * transform.up) / 1000f;
                        Gizmos.color = Color.blue;
                        Gizmos.DrawLine(transform.position, transform.position + force);
                    }

                    Gizmos.color = isGrounded ? Color.green : Color.red;
                    Gizmos.DrawSphere(position, .03f);
                }
            } 
        }

    }



}
