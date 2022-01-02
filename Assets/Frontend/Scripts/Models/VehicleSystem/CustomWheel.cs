using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Frontend.Scripts
{
    public class CustomWheel : MonoBehaviour
    {
       [SerializeField] private MeshCollider attachedCollider;

        public List<Collision> allCollisions = new List<Collision>();


        private bool isColliding;
        public bool IsColliding => isColliding;

      

        public bool HasContact()
        {
            Collision col = allCollisions[0];
            return col.contactCount > 0;
        }
        public ContactPoint CollisionReturnCol()
        {
            Collision col = allCollisions[0];
            return col.contacts[0];
        }
        private void Start()
        {
        }

        private void Update()
        {
            isColliding = allCollisions.Count > 0;
        }
        private void OnDrawGizmos()
        {
            Gizmos.color = isColliding ? Color.red : Color.blue;
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
            if(collision.collider.transform.root != transform.root)
            allCollisions.Add(collision);
            Debug.Log("fdsfs");
        }
        private void OnCollisionStay(Collision collision)
        {
            if(!allCollisions.Contains(collision))
            {
                if (collision.collider.transform.root != transform.root)
                    allCollisions.Add(collision);
            }
           
        }


        private void OnCollisionExit(Collision collision)
        {
            allCollisions.Remove(collision);
        }
    }
}
