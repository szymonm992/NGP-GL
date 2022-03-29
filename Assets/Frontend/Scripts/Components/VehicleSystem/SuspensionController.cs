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

        [Inject] public readonly TankStats tankStats;
        [Inject] public readonly IPlayerInput playerInputs;

        [SerializeField] private Text speedometer;
        [SerializeField] private Transform centerOfMass;

        #region LOCAL CONTROL VARIABLES
        private Vector2 inputs;
        private float combinedInput = 0f;
        private bool brake = false;
        private float currentSpeed = 0f;
        private float finalPower = 0f;
        #endregion

        [SerializeField] private UnderWheel[] wheelColliders;

        public float Speed => currentSpeed;

        private void Start()
        {
            finalPower = CalculateFinalPower();
            if (centerOfMass != null)
            {
                rig.centerOfMass = centerOfMass.localPosition;
            }

        }


        private void Update()
        {
            inputs = new Vector2(playerInputs.Horizontal, playerInputs.Vertical);
            combinedInput = Mathf.Abs(playerInputs.Horizontal) + Mathf.Abs(playerInputs.Vertical);
            brake = playerInputs.Brake;
            currentSpeed = rig.velocity.magnitude * 4f;
            speedometer.text = (currentSpeed).ToString("F0");

            UpdateWheels();

        }


        private void UpdateWheels()
        {
            float averageRPM = 0;
            if (wheelColliders.Length > 0)
            {
                foreach (UnderWheel wheelCollider in wheelColliders)
                {
                    wheelCollider.MotorTorque = 0;
                    Brakes(wheelCollider);
                    averageRPM += wheelCollider.RPM;
                }
                averageRPM /= wheelColliders.Length;
            }
            //Debug.Log("Average RPM is " + averageRPM);
        }


        private void Brakes(UnderWheel wheelCollider)
        {
            bool anyInput = combinedInput > 0;

            if (brake)
                wheelCollider.BrakeTorque = tankStats.BrakeTorque * 2f;
            else if (anyInput && !brake)
                wheelCollider.BrakeTorque = 0;
            else
                wheelCollider.BrakeTorque = tankStats.BrakeTorque;
        }
        private float CalculateFinalPower()
        {
            float one = (tankStats.EngineHP / rig.drag);
            float two = one / (rig.mass / 1000f);

            return (two * rig.mass * 4f);
        }

        private void OnDrawGizmos()
        {
            if (Application.isPlaying)
            {
                //drawing center of mass
                Gizmos.color = Color.green;
                Gizmos.DrawSphere(rig.worldCenterOfMass, .2f);
            }

        }
    }
}
