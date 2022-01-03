using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Frontend.Scripts
{
    public class CustomWheel : MonoBehaviour
    {
        [SerializeField] private MeshCollider attachedCollider;

        public List<ContactPoint> allCollisions = new List<ContactPoint>();
        Rigidbody rig;

        private bool isColliding;
        public bool IsColliding => isColliding;

        public bool move;

        public ContactPoint[] GetCollision()
        {
            if (allCollisions.Count > 0)
            {
                return allCollisions.ToArray();
            }
            return null;

        }

        private void Start()
        {
            rig = GetComponent<Rigidbody>();
        }

        private void FixedUpdate()
        {
            if (move)
            {
                rig.MovePosition(transform.position - transform.right * Time.deltaTime * 0.5f);
            }
        }

        private void Update()
        {
            isColliding = allCollisions.Count > 0;
        }
        private void OnDrawGizmos()
        {
            Gizmos.color = isColliding ? Color.blue : Color.green;

            Gizmos.DrawWireMesh(attachedCollider.sharedMesh, 0,
                attachedCollider.transform.position,
                attachedCollider.transform.rotation,
                attachedCollider.transform.lossyScale);

            ContactPoint[] col = GetCollision();
            if (col != null && col.Length > 0)
            {
                foreach (ContactPoint cp in col)
                {
                    Gizmos.color = Color.white;
                    Gizmos.DrawWireSphere(cp.point, .02f);
                }

            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.collider.transform.root != transform.root)
            {

                if (!allCollisions.Contains(collision.contacts[0]))
                {
                    allCollisions.Add(collision.contacts[0]);
               
                }


            }
        }
        private void OnCollisionStay(Collision collision)
        {
            if (collision.collider.transform.root != transform.root)
            {
                foreach (ContactPoint cp in collision.contacts)
                {
                    if (!allCollisions.Contains(collision.contacts[0]))
                    {
                        allCollisions.Add(collision.contacts[0]);
                    
                    }
                }

            }
        }


        private void OnCollisionExit(Collision collision)
        {
            
          
        }
    }
}
