using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UInc.Core.Utilities;
using UnityEngine;

namespace Frontend.Scripts
{
    public class CustomWheel : MonoBehaviour
    {
        
        [SerializeField] private bool enableHitPointGizmos = true;
        [SerializeField] private float wheelThickness = .08f;
        private bool isColliding;
        public bool IsColliding => isColliding;

        private Dictionary<Collider, Vector3> hitPoints = new();

        public Vector3[] GetHitPoints()
        {
            return hitPoints.Values.ToArray();
        }


        private void OnDrawGizmos()
        {
            Color gizmosColor = isColliding ? Color.blue : Color.green;
            Gizmos.color = gizmosColor;

            DebugExtension.DrawCircle(transform.position+transform.up * (wheelThickness/2),transform.up, gizmosColor, transform.lossyScale.x/2);
            DebugExtension.DrawCircle(transform.position - transform.up * (wheelThickness/2), transform.up, gizmosColor, transform.lossyScale.x / 2);

            if(enableHitPointGizmos)
            {
                foreach (KeyValuePair<Collider, Vector3> cp in hitPoints)
                {
                    if (cp.Value != Vector3.zero)
                    {
                        Gizmos.color = Color.red;
                        Gizmos.DrawSphere(cp.Value, .02f);
                    }
                }
            }

            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, transform.position+ transform.up*wheelThickness/2);
            Gizmos.DrawLine(transform.position, transform.position- transform.up*wheelThickness/2);
        }

        private void AddOrUpdateCollision(Collision collision)
        {
            Collider col = collision.collider;
            ContactPoint cp = collision.contacts[0];
            
            if (!hitPoints.ContainsKey(col))
            {
                hitPoints.Add(col, cp.point);
            }
            else if (hitPoints[col] != cp.point)
            {
                hitPoints[col] = cp.point;
            }
        }

        private void DeleteCollision(Collision collision)
        {
            Collider col = collision.collider;

            if (hitPoints.ContainsKey(col))
            {
                hitPoints.Remove(col);
            }
        }



        #region EVENT TRIGGERS

        private void OnCollisionEnter(Collision collision)
        {
          
            float distance = Vector3.Distance(transform.position, collision.contacts[0].point);
            DebugExtension.DebugPoint(transform.position, Color.yellow, .02f);
            float 

            Debug.Break();

            AddOrUpdateCollision(collision);

            if (!isColliding)
                isColliding = true;
        }
        private void OnCollisionStay(Collision collision)
        {
            AddOrUpdateCollision(collision);

            if (!isColliding)
            isColliding = true;
        }

        private void OnCollisionExit(Collision collision)
        {
            DeleteCollision(collision);

            if(hitPoints.Count == 0)
            isColliding = false;
        }

        #endregion
    }
}
