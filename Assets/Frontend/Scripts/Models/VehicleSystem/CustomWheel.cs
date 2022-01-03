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

      

        public Collision GetCollision()
        {
            if(allCollisions.Count>0)
            {
                return allCollisions[0];
            }
            return null;
            
        }
        public Collision CollisionReturnCol()
        {
            return allCollisions[0];
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
            Gizmos.color = isColliding ? Color.blue : Color.green;

            Gizmos.DrawWireMesh(attachedCollider.sharedMesh, 0,
                attachedCollider.transform.position,
                attachedCollider.transform.rotation,
                attachedCollider.transform.lossyScale);

            Collision col = GetCollision();
            if(col != null)
            {
                Gizmos.color = Color.white;
                Gizmos.DrawWireSphere(col.contacts[0].point, .2f);
                Debug.Log(col.contacts[0].point);
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (!allCollisions.Contains(collision))
            {
                if (collision.collider.transform.root != transform.root)
                {
                    allCollisions.Add(collision);
                    isColliding = true;
                }
                    
            }
        }
        private void OnCollisionStay(Collision collision)
        {
            if(!allCollisions.Contains(collision))
            {
                if (collision.collider.transform.root != transform.root)
                {
                    allCollisions.Add(collision);
                    isColliding = true;
                }
                    
            }
           
        }


        private void OnCollisionExit(Collision collision)
        {
            allCollisions.Remove(collision);
            if (allCollisions.Count == 0)
                isColliding = false;
        }
    }
}
