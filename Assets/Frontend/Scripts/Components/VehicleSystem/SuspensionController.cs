using Frontend.Scripts.Enums;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

using Frontend.Scripts.Models.VehicleSystem;

namespace Frontend.Scripts.Components.VehicleSystem
{
    public class SuspensionController : MonoBehaviour
    {

        [Inject(Id = "mainRig")] private readonly Rigidbody rig;
        [Inject] public IVehicleStats tankStats;
        [Inject] public IPlayerInput playerInputs;

        private Vector2 inputs;
        private bool brake;

        private void Start()
        {
        }

        private void FixedUpdate()
        {
            
        }

        private void Update()
        {
            inputs = new Vector2(playerInputs.Horizontal, playerInputs.Vertical);
            brake = playerInputs.Brake;
        }
    }
}
