using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Frontend.Scripts
{
    public class CustomWheel : MonoBehaviour
    {
        [SerializeField] private MeshCollider attachedCollider;

        public List<ContactPoint> allCollisions = new List<ContactPoint>();

        private bool isColliding;
        public bool IsColliding => isColliding;

        private Dictionary<Collider, Vector3> hitPoint = new();
        private List<Vector3> hitNormal = new();
        

        public ContactPoint[] GetCollision()
        {
            if (allCollisions.Count > 0)
            {
                return allCollisions.ToArray();
            }
            return null;
        }

        private void OnDrawGizmos()
        {
            Color gizmosColor = isColliding ? Color.blue : Color.green;
            Gizmos.color = gizmosColor;

           // Gizmos.DrawWireMesh(attachedCollider.sharedMesh, 0,
           // attachedCollider.transform.position,
           // attachedCollider.transform.rotation,
           // attachedCollider.transform.lossyScale);


            DebugExtension.DrawCircle(transform.position+transform.up * (transform.lossyScale.y),transform.up, gizmosColor, .15f);
            DebugExtension.DrawCircle(transform.position - transform.up * (transform.lossyScale.y), transform.up, gizmosColor, .15f);

            foreach (KeyValuePair<Collider, Vector3> cp in hitPoint)
            {
                if (cp.Value != Vector3.zero)
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawSphere(cp.Value, .03f);
                }
            }
           
        }

        private void AddCollision(Collision collision)
        {
            Collider col = collision.collider;
            ContactPoint cp = collision.contacts[0];
            if (!hitPoint.ContainsKey(col))
            {
                hitPoint.Add(col, cp.point);
                // hitNormal.Add(cp.normal);
            }
            else if (hitPoint[col] != cp.point)
            {
                hitPoint[col] = cp.point;
            }
        }

        private void DeleteCollision(Collision collision)
        {
            Collider col = collision.collider;

            if (hitPoint.ContainsKey(col))
            {
                hitPoint.Remove(col);
                // hitNormal.Remove(cp.normal);
            }
        }



        #region EVENT TRIGGERS

        private void OnCollisionEnter(Collision collision)
        {
            AddCollision(collision);

            if (!isColliding)
                isColliding = true;
        }
        private void OnCollisionStay(Collision collision)
        {
            AddCollision(collision);

            if (!isColliding)
            isColliding = true;
        }

        private void OnCollisionExit(Collision collision)
        {
            DeleteCollision(collision);

            if(hitPoint.Count == 0)
            isColliding = false;
        }

        #endregion
    }
}
