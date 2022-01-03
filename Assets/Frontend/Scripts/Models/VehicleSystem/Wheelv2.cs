using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Frontend.Scripts
{
    public class Wheelv2 : MonoBehaviour
    {
        private bool isColliding;
        public bool IsColliding => isColliding;

        public Mesh mesh;

        RaycastHit[] allHits;
       
        private void OnDrawGizmos()
        {
            isColliding = CheckCylinder(transform.position, transform.position + transform.up, .5f, LayerMask.GetMask("Default"), out allHits, QueryTriggerInteraction.Collide);

            Gizmos.color = isColliding ? Color.blue : Color.green;
            Gizmos.DrawWireMesh(mesh, 0, transform.position + transform.up * 0.5f, transform.rotation, new Vector3(1, .5f, 1));
        }


        private void FixedUpdate()
        {
            if (!isColliding) return;
            foreach (RaycastHit hittt in allHits)
            {
                DebugExtension.DebugWireSphere(hittt.point, .1f);
            }
        }



        public static bool CheckCylinder(Vector3 aStart, Vector3 aEnd, float aRadius, int aLayerMask,out RaycastHit[] hits, QueryTriggerInteraction aQueryTriggerInteraction)
        {
            
            Vector3 dir = aEnd - aStart;
            bool overlappingCapsule = Physics.CheckCapsule(aStart, aEnd, aRadius, aLayerMask, aQueryTriggerInteraction);

             //hits = Physics.CapsuleCastAll(aStart, aEnd, aRadius, dir, 1f);
            //Gizmos.color = overlappingCapsule ? Color.blue : Color.green;
            //DebugExtension.DebugCapsule(aStart - (dir.normalized * aRadius), aEnd + (dir.normalized * aRadius), overlappingCapsule ? Color.blue : Color.green, aRadius);

            if (!overlappingCapsule)
            {
                hits = new RaycastHit[] { };
                return false;
            }
                
            Quaternion q = Quaternion.LookRotation(dir);
            Quaternion q2 = Quaternion.AngleAxis(45f, dir);
            Vector3 size = new Vector3(aRadius, aRadius / (1f + Mathf.Sqrt(2f)), dir.magnitude * 0.5f);
            for (int i = 0; i < 4; i++)
            {
               hits = Physics.BoxCastAll(aStart + dir * 0.5f, size, dir, q, 0, aLayerMask, aQueryTriggerInteraction);

                bool boxOverllaping = hits.Length > 0;
               // Gizmos.color = boxOverllaping ? Color.blue : Color.green;
               // Gizmos.DrawWireCube(aStart + dir * .5f, size);

                if (boxOverllaping)
                {
                    return true;
                }
                q = q2 * q;
            }
            hits = new RaycastHit[] { };
            return false;
        }
       
    }


    
}
