using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Kit;
namespace Frontend.Scripts
{
    public class UnderWheel : MonoBehaviour
    {
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

        private void OnDrawGizmos()
        {
            /*
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, transform.position + transform.up * -springLength);
            Gizmos.color = Color.red;
          
            Gizmos.DrawWireSphere(transform.position + transform.up * -springLength, wheelRadius);*/
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

        }


    }



}
