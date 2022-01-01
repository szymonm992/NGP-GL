using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Frontend.Scripts
{
    public class PCInput : MonoBehaviour, IPlayerInput
    {
        private float horizontal;
        private float vertical;

        private bool brake;

        public float Vertical => vertical;
        public float Horizontal => horizontal;
        public bool Brake => brake;


        private void Update()
        {
            horizontal = Input.GetAxis("Horizontal");
            vertical = Input.GetAxis("Vertical");
            brake = Input.GetButton("Brake");
        }
    }
}
