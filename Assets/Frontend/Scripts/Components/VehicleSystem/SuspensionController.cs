using Frontend.Scripts.Enums;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Frontend.Scripts.Components.VehicleSystem
{
    public class SuspensionController : MonoBehaviour
    {

        [Inject(Id = "mainRig")] private readonly Rigidbody rig;


        private void Start()
        {
            
        }

        private void FixedUpdate()
        {
            
        }

        private void Update()
        {
            
        }
    }
}
