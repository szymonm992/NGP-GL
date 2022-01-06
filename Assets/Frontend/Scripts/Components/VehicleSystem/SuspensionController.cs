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

        #region LOCAL CONTROL VARIABLES

        public ConfigurableJoint[] joints;
        private Vector2 inputs;
        private bool brake;
        private float currentSpeed;
        private float finalPower;
        #endregion

        private void Start()
        {
            finalPower = CalculateFinalPower();
            Debug.Log(finalPower);
        }

        private void FixedUpdate()
        {
            ApplyForcesToWheels();
            
        }
        private void Update()
        {
            inputs = new Vector2(playerInputs.Horizontal, playerInputs.Vertical);
            brake = playerInputs.Brake;
            currentSpeed = rig.velocity.magnitude * 4f;
            speedometer.text = (currentSpeed).ToString("F0");
            
        }

        private float CalculateFinalPower()
        {
            float one = (tankStats.EngineHP / rig.drag);
            float two = one / (rig.mass / 1000f);

            return (two * rig.mass * 4);
        }
        private void ApplyForcesToWheels()
        {
            foreach(ConfigurableJoint joint in joints)
            {
                float evaluatedFromCurve = tankStats.EnginePowerCurve.Evaluate(currentSpeed);
                rig.AddForceAtPosition(finalPower/6 * evaluatedFromCurve * inputs.y * Time.deltaTime * transform.forward, joint.transform.position);
            }
            
        }
    }
}
