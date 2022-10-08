using Frontend.Scripts.Interfaces;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Frontend.Scripts
{
    public class UTTankSteering : MonoBehaviour, IVehicleSteering
    {
        [Inject(Id = "mainRig")] private Rigidbody rig;
        [Inject] private readonly IVehicleController suspensionController;
        [Inject] private readonly IPlayerInputProvider inputProvider;

        [SerializeField] private float steerSpeed;
        private float steerInput;
        private float currentSteerSpeed;
        public void SetSteeringInput(float input)
        {
            steerInput = input;
        }

        private void Update()
        {
            SetSteeringInput(inputProvider.Horizontal);
        }


        private void FixedUpdate()
        {
            currentSteerSpeed = steerSpeed;
            if(inputProvider.CombinedInput > 1) currentSteerSpeed  *= 1 / 1.4f;
            if (Mathf.Abs(rig.angularVelocity.y) < 1f)
            {
                rig.AddRelativeTorque((Vector3.up * steerInput) * currentSteerSpeed * 50 * Time.deltaTime, ForceMode.Acceleration);
                //Debug.Log(rig.angularVelocity.magnitude);
            }

        }
    }
}
