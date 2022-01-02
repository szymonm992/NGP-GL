using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Frontend.Scripts
{
    public class CustomWheel : MonoBehaviour
    {
       [SerializeField] private MeshCollider attachedCollider;

        private bool isColliding = false;
        public List<Collision> allCollisions = new();

        public bool IsColliding => allCollisions.Count > 0;
        private void Start()
        {
            attachedCollider = GetComponent<MeshCollider>();
        }
        private void OnDrawGizmos()
        {
            Gizmos.color = isColliding ? Color.blue : Color.green;
            Gizmos.DrawWireMesh(attachedCollider.sharedMesh, 0,
                attachedCollider.transform.position,
                attachedCollider.transform.rotation,
                attachedCollider.transform.localScale);

            if (allCollisions.Count>0)
            {
                foreach (Collision col in allCollisions)
                {
                    foreach (ContactPoint cp in col.contacts)
                    {
                        Gizmos.color = Color.red;

                        Gizmos.DrawSphere(cp.point, .015f);
                    }

                }
            }
               
                
        }

        private void OnCollisionEnter(Collision collision)
        {
            allCollisions.Add(collision);
        }


        private void OnCollisionExit(Collision collision)
        {
            allCollisions.Remove(collision);
        }
    }
}
