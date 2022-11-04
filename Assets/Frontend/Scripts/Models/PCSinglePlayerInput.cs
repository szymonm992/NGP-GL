using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

using Frontend.Scripts.Interfaces;

namespace Frontend.Scripts.Models
{
    public class PCSinglePlayerInput : MonoBehaviour, IPlayerInputProvider
    {
        [Inject] private readonly IVehicleController controller;
        private float horizontal;
        private float vertical;

        private bool brake;

        private float combinedInput = 0f;

        public float Vertical => vertical;
        public float Horizontal => horizontal;
        public bool Brake => brake;

        public float CombinedInput => combinedInput;

        public float RawHorizontal => ReturnRawInput(ref horizontal);
        public float RawVertical => ReturnRawInput(ref vertical);
        public float AbsoluteVertical => ReturnAbsoluteVertical(ref vertical);
        public float AbsoluteHorizontal => ReturnAbsoluteHorizontal(ref horizontal);
        public float SignedVertical => ReturnSignedInput(ref vertical);
        public float SignedHorizontal => ReturnSignedInput(ref horizontal);

        private float ReturnRawInput(ref float input)
        {
            if(input != 0)
            {
                if (input > 0) return 1f;
                else return -1f;
            }
            return 0;
        } 

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
            return Mathf.Sign(input);
        }

        private void Update()
        {
            brake = Input.GetButton("Brake");
            horizontal = !Brake ? Input.GetAxis("Horizontal") : 0f;
            vertical = !Brake ? Input.GetAxis("Vertical") : 0f;
           

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
