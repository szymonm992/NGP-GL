using Frontend.Scripts.Enums;
using Frontend.Scripts.Models;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Frontend.Scripts.Components
{
    public class UTWheel : MonoBehaviour
    {
        private Rigidbody rig;

        [SerializeField]
        private UnderWheelDebug debugSettings = new UnderWheelDebug()
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

        private float motorTorque = 0f;
        private float brakeTorque = 0f;
        private float steerAngle = 0f;

        private bool isGrounded = false;


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

        private void FixedUpdate()
        {
            Vector3 newPosition = GetTirePosition();
            Vector3 newVelocity = (newPosition - tirePosition) / Time.fixedDeltaTime;
            tirePosition = newPosition;
            normalForce = GetSuspensionForce(tirePosition) + mass * Mathf.Abs(Physics.gravity.y);

            float longitudalSpeed = Vector3.Dot(newVelocity, transform.forward);
            float lateralSpeed = Vector3.Dot(newVelocity, -transform.right);

            //float lateralForce = GetLateralForce(normalForce, lateralSpeed, longitudalSpeed);
           // float longitudalForce = GetLongitudalForce(normalForce, longitudalSpeed);


            finalForce = normalForce * transform.up;

            if (!isGrounded) return;

            rig.AddForceAtPosition(finalForce, tirePosition);

           // UpdateAngularVelocity(longitudalForce);
        }

        public void GetWorldPosition(out Vector3 position, out Quaternion rotation)
        {
            var localTireRotation = Quaternion.Euler(tireRotation * Mathf.Rad2Deg, 0f, 0f);
            position = GetTirePosition();
            rotation = transform.rotation * localTireRotation;
        }

        public Vector3 GetTirePosition()
        {
            isGrounded = Physics.Raycast(new Ray(transform.position, -transform.up), out RaycastHit hit, wheelRadius + suspensionTravel);
            if (isGrounded)
            {
                compression = hit.distance - wheelRadius;
                return hit.point + (transform.up * wheelRadius);
            }
            else
            {
                compression = suspensionTravel;
                return transform.position - (transform.up * suspensionTravel);
            }
        }

        private float GetSuspensionForce(Vector3 localTirePosition)
        {
            float distance = Vector3.Distance(transform.position - transform.up * suspensionTravel, localTirePosition);
            float springForce = spring * distance;
            float damperForce = damper * ((distance - previousSuspensionDistance) / Time.fixedDeltaTime);
            previousSuspensionDistance = distance;
            return springForce + damperForce;
        }

    }
}
