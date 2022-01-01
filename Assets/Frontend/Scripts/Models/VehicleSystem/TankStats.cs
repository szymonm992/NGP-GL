using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Frontend.Scripts.Models.VehicleSystem
{
    public class TankStats : MonoBehaviour, IVehicleStats
    {
        [SerializeField] private string tankName;

        [SerializeField] private float maxForwardSpeed;
        [SerializeField] private float maxBackwardsSpeed;

        public string TankName => tankName;
        public float MaxForwardSpeed => maxForwardSpeed;
        public float MaxBackwardsSpeed => maxBackwardsSpeed;
    }
}
