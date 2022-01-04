using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Kit;
namespace Frontend.Scripts
{
    public class UnderWheel : MonoBehaviour
    {
        private bool isColliding;

       [SerializeField] private float wheelRadius = .15f;
        [SerializeField] private float suspensionTravelRadius = .15f;

        public LayerMask wheelMask;
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.white;
            Vector3 topBorder = transform.position + transform.up * suspensionTravelRadius;
            Vector3 bottomBorder = transform.position - transform.up * suspensionTravelRadius;
          

            Gizmos.DrawLine(topBorder - transform.forward * .03f, topBorder + transform.forward * .03f);
            Gizmos.DrawLine(bottomBorder - transform.forward * .03f, bottomBorder + transform.forward * .03f);
            Gizmos.DrawSphere(transform.position, .02f);

            Gizmos.DrawLine(topBorder, bottomBorder);

            Vector3 direction = -transform.up;

            RaycastHit rayHit;

            Ray rayTopToBottom = new Ray(topBorder, direction);
            Ray rayBottomToTop = new Ray(bottomBorder, -direction);

            Vector3 sphereCenterPoint = bottomBorder;

            if (Physics.SphereCast(rayTopToBottom, wheelRadius, out rayHit, suspensionTravelRadius * 2f, wheelMask) ||
                (Physics.SphereCast(rayBottomToTop, wheelRadius, out rayHit, suspensionTravelRadius * 2f, wheelMask)))
            {
                sphereCenterPoint = transform.position + direction * (rayHit.distance - wheelRadius);

                Gizmos.color = Color.red;
                Gizmos.DrawSphere(rayHit.point, .03f);
            }
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(sphereCenterPoint, wheelRadius);

        }




    }



}
