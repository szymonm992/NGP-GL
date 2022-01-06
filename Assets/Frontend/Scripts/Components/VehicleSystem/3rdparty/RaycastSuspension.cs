using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Frontend.Scripts
{
    public class RaycastSuspension : MonoBehaviour
    {

        private Rigidbody rig; 

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
        private float verticalForce;

        private Vector3 suspensionForce;
        private Vector3 wheelVelocityLocal;

        public float wheelRadius;

        private bool isColliding;
        private void Start()
        {
            rig = transform.root.GetComponent<Rigidbody>();

            minLength = restLength - springTravel;
            maxLength = restLength + springTravel;
            springLength = maxLength;
        }


        private void Update()
        {
            Debug.DrawRay(transform.position-transform.up*springLength, -transform.up * wheelRadius, Color.blue);
            Debug.DrawRay(transform.position, -transform.up * springLength, isColliding ? Color.green : Color.red);
        }
        private void FixedUpdate()
        {
            isColliding = (Physics.Raycast(transform.position, -transform.up, out RaycastHit hit, maxLength + wheelRadius));

            if(isColliding)
            {
                lastLength = springLength;
                springLength = hit.distance - wheelRadius;
                springLength = Mathf.Clamp(springLength, minLength, maxLength);
                springVelocity = (lastLength - springLength) / Time.fixedDeltaTime;

                springForce = springStiffness * (restLength - springLength);
                damperForce = damperStiffness * springVelocity;

                suspensionForce = (springForce + damperForce) * transform.up;

                //local to world space calculation of velocity
                wheelVelocityLocal = transform.InverseTransformDirection(rig.GetPointVelocity(hit.point));

                verticalForce = Input.GetAxis("Vertical") * springForce;


                rig.AddForceAtPosition(suspensionForce + (verticalForce * transform.forward) , hit.point);
            }
        }





    }
}

