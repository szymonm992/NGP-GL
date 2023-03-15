using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Frontend.Prototyping.Scripts
{
    [RequireComponent(typeof(Rigidbody))]
    public class MovingPlatform : MonoBehaviour
    {
        public enum MovementType
        {
            None,
            LineSine,
            Circle,
        }

        [SerializeField] private MovementType movementType;
        [SerializeField] private float movementScale;
        [SerializeField] private float movementSpeed;

        private Vector3 originPosition;
        private Vector3 lastPosition;

        private Rigidbody rigid;

        private void Start()
        {
            // prototyping purposes only. I'll care about zenjecting it later
            rigid = GetComponent<Rigidbody>();

            originPosition = transform.position;
            lastPosition = originPosition;
        }

        private void Update()
        {
            transform.position = originPosition + GetMovementOffset();
        }

        private void FixedUpdate()
        {
            var targetPosition = originPosition + GetMovementOffset();
            rigid.velocity = (targetPosition - lastPosition) / Time.fixedDeltaTime;
            lastPosition = targetPosition;
        }

        private Vector3 GetMovementOffset()
        {
            float factor = Time.time * movementSpeed;

            Vector3 offset = movementType switch
            {
                MovementType.LineSine => transform.forward * (Mathf.Sin(factor) * 0.5f + 0.5f),
                MovementType.Circle => Quaternion.AngleAxis(factor, transform.up) * transform.forward,
                _ => Vector3.zero
            };

            return offset * movementScale;
        }
    }
}
