using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

using Frontend.Scripts.Interfaces;

namespace Frontend.Scripts.Models
{
    public class PCSinglePlayerInput : MonoBehaviour, IPlayerInput
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

        public float RawHorizontal => ReturnRawValue(ref horizontal);
        public float RawVertical => ReturnRawValue(ref vertical);

        private float ReturnRawValue(ref float input)
        {
            if(input != 0)
            {
                if (input > 0) return 1f;
                else return -1f;
            }
            return 0;
        }
        private void Update()
        {
            horizontal = Input.GetAxis("Horizontal");
            vertical = Input.GetAxis("Vertical");
            brake = Input.GetButton("Brake");

            combinedInput = Mathf.Abs(horizontal) + Mathf.Abs(vertical);
        }

        private void FixedUpdate()
        {
            if (combinedInput > 0)
            {
                controller.MovementLogic();
            }

            if (horizontal != 0)
            {
                controller.TurningLogic();
            }
        }
    }
}
