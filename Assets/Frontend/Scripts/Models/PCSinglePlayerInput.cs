using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

using Frontend.Scripts.Interfaces;

namespace Frontend.Scripts.Models
{
    public class PCSinglePlayerInput : MonoBehaviour, IPlayerInputProvider
    {
        private float horizontal;
        private float vertical;

        private bool brake;

        private float combinedInput = 0f;
        private float lastVerticalInput = 0f;

        public float Vertical => vertical;
        public float Horizontal => horizontal;
        public bool Brake => brake;

        public float CombinedInput => combinedInput;

        public float AbsoluteVertical => ReturnAbsoluteVertical(ref vertical);
        public float AbsoluteHorizontal => ReturnAbsoluteHorizontal(ref horizontal);
        public float SignedVertical => ReturnSignedInput(ref vertical);
        public float SignedHorizontal => ReturnSignedInput(ref horizontal);

        public float LastVerticalInput => lastVerticalInput;

        private float ReturnAbsoluteHorizontal (ref float input)
        {
            return Mathf.Abs(input);
        }

        private float ReturnAbsoluteVertical(ref float input)
        {
            return Mathf.Abs(input);
        }

        private float ReturnSignedInput(ref float input)
        {
            return input != 0 ? Mathf.Sign(input) : 0f;
        }

        private void Update()
        {
            brake = Input.GetButton("Brake");
            horizontal = !Brake ? Input.GetAxis("Horizontal") : 0f;
            vertical = !Brake ? Input.GetAxis("Vertical") : 0f;

            if (vertical != 0)
            {
                lastVerticalInput = SignedVertical;
            }

            combinedInput = Mathf.Abs(horizontal) + Mathf.Abs(vertical);
        }

        private void FixedUpdate()
        {
            if (combinedInput > 0)
            {
                //controller?.MovementLogic();
            }

            if (horizontal != 0)
            {
                //controller?.TurningLogic();
            }
        }
    }
}
