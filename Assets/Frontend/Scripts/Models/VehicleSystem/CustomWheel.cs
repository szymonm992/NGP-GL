using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Frontend.Scripts
{
    public class CustomWheel : MonoBehaviour
    {
       [SerializeField] private MeshCollider collider;

        private bool isColliding = false;
        private List<Collider> allColliders = new();

        private void Start()
        {
            collider = GetComponent<MeshCollider>();
        }
        private void OnDrawGizmos()
        {
            Gizmos.color = isColliding ? Color.blue : Color.green;
            Gizmos.DrawWireMesh(collider.sharedMesh, 0, collider.transform.position, collider.transform.rotation, collider.transform.localScale);
        }

        private void OnTriggerEnter(Collider other)
        {
            allColliders.Add(other);
            CheckColliding(ref isColliding);
        }

        private void OnTriggerExit(Collider other)
        {
            allColliders.Remove(other);
            CheckColliding(ref isColliding);
        }
        private void CheckColliding(ref bool isCol)
        {
            if (isCol && allColliders.Count == 0)
                isCol = false;
            else if (!isCol & allColliders.Count > 0)
                isCol = true;
        }
    }
}
