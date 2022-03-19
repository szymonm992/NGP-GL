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
        [SerializeField] private float wheelThickness = .8f;
        [SerializeField] private float radius = .5f;
        private bool isColliding;
        public bool IsColliding => isColliding;

        private Dictionary<Collider, Vector3> hitPoints = new();
        private (Vector3, Vector3) cylinderCaps;

        public Vector3[] GetHitPoints()
        {
            return hitPoints.Values.ToArray();
        }
        private void OnValidate()
        {
            CreateCylinder();
        }
        private void CreateCylinder()
        {
            UpdateCylinder();
        }
        private void OnDrawGizmos()
        {
            Color gizmosColor = isColliding ? Color.blue : Color.green;
            Gizmos.color = gizmosColor;

            DebugExtension.DrawCircle(transform.position+transform.up * (wheelThickness/2),transform.up, gizmosColor, transform.lossyScale.x/2);
            DebugExtension.DrawCircle(transform.position - transform.up * (wheelThickness/2), transform.up, gizmosColor, transform.lossyScale.x / 2);

            Gizmos.DrawSphere(cylinderCaps.Item1, .03f);
            Gizmos.DrawSphere(cylinderCaps.Item2, .03f);

            if (enableHitPointGizmos)
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

        private void AddOrUpdateCollision(Collision collision, Vector3 point)
        {
            Collider col = collision.collider;
            ContactPoint cp = collision.contacts[0];
            
            if (!hitPoints.ContainsKey(col))
            {
                hitPoints.Add(col, point);
            }
            else if (hitPoints[col] != point)
            {
                hitPoints[col] = point;
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

        private void UpdateCylinder()
        {
            cylinderCaps = new(transform.position + transform.up * wheelThickness/2, transform.position - transform.up * wheelThickness/2);
        }

        #region EVENT TRIGGERS

        private void CheckCollision(Collision collision)
        {
            Debug.Log(collision.contacts.Length);

            for (int i = 0; i < collision.contactCount; i++)
            {
                Vector3 point = collision.contacts[i].point + (transform.position - collision.contacts[i].point) * .05f;
                if (IsPointInsideCylinder(point, cylinderCaps.Item1, cylinderCaps.Item2, radius))
                {
                    AddOrUpdateCollision(collision, collision.contacts[i].point);

                    if (!isColliding)
                        isColliding = true;
                    break;
                }
                else
                {
                    DebugExtension.DebugPoint(collision.contacts[i].point, Color.yellow, 1f);
                }
            }
        }

        private void Update()
        {
           if(transform.hasChanged)
            {
                UpdateCylinder();
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
       
            CheckCollision(collision);
        }
        private void OnCollisionStay(Collision collision)
        {
 
            CheckCollision(collision);
        }

        private void OnCollisionExit(Collision collision)
        {
            DeleteCollision(collision);

            if(hitPoints.Count == 0)
            isColliding = false;
        }

        #endregion
        private bool IsPointInsideCylinder(Vector3 p, Vector3 a, Vector3 b, float r)
        {
            Vector3 ba = b - a;
            Vector3 pa = p - a;
            float m = ba.x * ba.x + ba.y * ba.y + ba.z * ba.z;
            float n = pa.x * ba.x + pa.y * ba.y + pa.z * ba.z;
            Vector3 k = pa * m - ba * n;
            float x = Mathf.Sqrt(k.x * k.x + k.y * k.y + k.z * k.z) - r * m;
            float y = Mathf.Abs(n - m * 0.5f) - m * 0.5f;
            float x2 = x * x;
            float y2 = y * y * m;
            float d = (Mathf.Max(x, y) < 0.0f) ? -Mathf.Min(x2, y2) : (((x > 0.0f) ? x2 : 0.0f) + ((y > 0.0f) ? y2 : 0.0f));
            return ((Mathf.Sign(d) * Mathf.Sqrt(Mathf.Abs(d)) / m) < 0.0f);
        }

    }



}
