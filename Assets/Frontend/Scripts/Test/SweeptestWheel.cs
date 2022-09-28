using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Frontend.Scripts
{
    public class SweeptestWheel : MonoBehaviour
    {
        public Rigidbody rig;
        public bool detect=false;
        public RaycastHit hit;
        public LayerMask mask;
        private void FixedUpdate()
        {
            detect = rig.SweepTest(-transform.up, out hit, 1f);
        }

        private void OnDrawGizmos()
        {
            if(detect)
            {
                Gizmos.DrawSphere(hit.point, 0.1f);
            }
        }
    }
}
