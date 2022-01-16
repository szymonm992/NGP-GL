using Frontend.Scripts.Enums;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

using Frontend.Scripts.Models.VehicleSystem;

namespace Frontend.Scripts.Components.VehicleSystem
{
    public class SuspensionController : MonoBehaviour
    {

        [Inject(Id = "mainRig")] private readonly Rigidbody rig;

        [Inject] public readonly IVehicleStats tankStats;
        [Inject] public readonly IPlayerInput playerInputs;

        [SerializeField] private Text speedometer;
        [SerializeField] private Transform centerOfMass;

        #region LOCAL CONTROL VARIABLES
        private Vector2 inputs;
        private bool brake;
        private float currentSpeed;
        private float finalPower;
        #endregion

        [SerializeField] private UnderWheel[] wheelColliders;

        public float Speed => currentSpeed;

        private void Start()
        {
            finalPower = CalculateFinalPower();
            rig.centerOfMass = centerOfMass.localPosition;
        }

        
        private void Update()
        {
            inputs = new Vector2(playerInputs.Horizontal, playerInputs.Vertical);
            brake = playerInputs.Brake;
            currentSpeed = rig.velocity.magnitude * 4f;
            speedometer.text = (currentSpeed).ToString("F0");

            UpdateWheels();
            
        }


        private void UpdateWheels()
        {
            if (wheelColliders.Length > 0)
            {
                foreach (UnderWheel wheelCollider in wheelColliders)
                {
                    wheelCollider.MotorTorque = 0;
                    wheelCollider.BrakeTorque = brake ? tankStats.BrakeTorque : 0;
                }
            }
        }

        private float CalculateFinalPower()
        {
            float one = (tankStats.EngineHP / rig.drag);
            float two = one / (rig.mass / 1000f);

            return (two * rig.mass * 4f);
        }

        private void OnDrawGizmos()
        {
            if(Application.isPlaying)
            {
                //drawing center of mass
                Gizmos.color = Color.green;
                Gizmos.DrawSphere(rig.worldCenterOfMass, .2f);
            }
            
        }

    }
}
