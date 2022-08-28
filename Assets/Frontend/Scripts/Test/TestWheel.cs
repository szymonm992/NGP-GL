using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Frontend.Scripts
{
    public class TestWheel : MonoBehaviour
    {
        [SerializeField] private Transform[] wheelTransforms;
        [SerializeField] private Rigidbody rig;

        [SerializeField] private float suspensionRestDist;
        [SerializeField] private float springStrength;
        [SerializeField] private float springDamper;
        [SerializeField] private float wheelDiameter = 0.4f;
        [SerializeField] private float tireGripFactor = 1f;
        [SerializeField] private float tireMass = 20f;
        [SerializeField] private float carTopSpeed = 20f;
        private float inputY, inputX;

        [SerializeField] private AnimationCurve powerCurve;

        private void Update()
        {
            inputY = Input.GetAxis("Vertical");    
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
        }
    }
}
