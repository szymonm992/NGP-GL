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

            for(int i=0;i<collision.contactCount;i++)
            {
                DebugExtension.DebugPoint(collision.contacts[i].point, Color.yellow, 1f);
                float distanceToEdge = Vector3.Distance(transform.position - transform.up * wheelThickness / 2, transform.position);
                float distanceToPoint = Vector3.Distance(transform.position, collision.contacts[i].point);

                if (distanceToPoint <= 0.5f)
                {
                    Debug.Log("DISTTANCE confirmed");
                    AddOrUpdateCollision(collision);

                    if (!isColliding)
                        isColliding = true;
                    break;
                }
                else
                {

                    
                    Vector3 dirToContact = collision.contacts[i].point - transform.position;
                    // TODO DWA DIR TO CONTACT I SPRAWDZANIE KTÓRY JEST BLIZEJ PUNKTU I DOT ROBIÆ Z TYM PUNKTEM A(PLUSOWY I MINUSOWY)

                    Vector3 directionToEdgeNegative = transform.position - transform.up * wheelThickness / 2;
                    Vector3 directionToEdgePositive = transform.position + transform.up * wheelThickness / 2;

                    Vector3 desiredDirectionToEdge = Vector3.Distance(collision.contacts[i].point, directionToEdgePositive) > Vector3.Distance(collision.contacts[i].point, directionToEdgeNegative)
                        ? directionToEdgeNegative : directionToEdgePositive;

                    float dotProduct = (Vector3.Dot(dirToContact, desiredDirectionToEdge));
                   
                    DebugExtension.DebugPoint(transform.position - transform.up * wheelThickness / 2, Color.yellow, .02f);
                    if (dotProduct <= distanceToEdge)
                    {
                        Debug.Log("CONFIRMED Dp is " + dotProduct + " and distance is " + distanceToEdge);
                        AddOrUpdateCollision(collision);

                        if (!isColliding)
                            isColliding = true;
                        break;
                    }
                    else
                    {
                        Debug.Log("REJECTED Dp is " + dotProduct + " and distance is "+ distanceToEdge);
                    }
                   
                }
            }
            
           


            

           
        }
        private void OnCollisionStay(Collision collision)
        {
            
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
