using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Kit;
namespace Frontend.Scripts
{
    public class UnderWheel : MonoBehaviour
    {
        [SerializeField] private Rigidbody rig;


        public float restLength;
        public float springTravel;
        public float springStiffness;
        public float damperStiffness;

        private float minLength;
        private float maxLength;
        private float lastLength;
        private float springLength;
        private float springVelocity;
        private float springForce;
        private float damperForce;

        private Vector3 suspensionForce;

        public float wheelRadius;

       

        private void Start()
        {
           
            minLength = restLength - springTravel;
            maxLength = restLength + springTravel;

            springLength = maxLength;
        }

        private void FixedUpdate()
        {
            bool isColliding = Physics.Raycast(transform.position, -transform.up, out RaycastHit hit, maxLength + wheelRadius);

            if(isColliding)
            {
                lastLength = springLength;
                springLength = hit.distance - wheelRadius;
                springLength = Mathf.Clamp(springLength, minLength, maxLength);

                springVelocity = (lastLength - springLength) / Time.fixedDeltaTime;
                springForce = springStiffness * (restLength - springLength);
                damperForce = damperStiffness * springVelocity;

                suspensionForce = (springForce + damperForce) * transform.up;

                rig.AddForceAtPosition(suspensionForce, hit.point);

            }

            Debug.DrawRay(transform.position, -transform.up * springLength, isColliding ? Color.green : Color.red);

        }

    }



}
