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

        public bool IsColliding => isColliding;
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
                attachedCollider.transform.lossyScale);

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
            CheckColliding(ref isColliding);
        }


        private void OnCollisionExit(Collision collision)
        {
            allCollisions.Remove(collision);
            CheckColliding(ref isColliding);
        }
        private void CheckColliding(ref bool isCol)
        {
            if (isCol && allCollisions.Count == 0)
                isCol = false;
            else if (!isCol & allCollisions.Count > 0)
                isCol = true;
        }
    }
}
