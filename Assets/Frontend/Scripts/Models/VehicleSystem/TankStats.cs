using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Frontend.Scripts.Models.VehicleSystem
{
    public class TankStats : MonoBehaviour, IVehicleStats
    {
        [SerializeField] private string tankName;

        [Header("Movement parameters")]
        [SerializeField] private float maxForwardSpeed;
        [SerializeField] private float maxBackwardsSpeed;

        [Header("Engine parameters")]
        [SerializeField] private float engineHP;
        [SerializeField] private float drag;


        [Header("Curves")]
        [SerializeField] private AnimationCurve engineCurve;

        public string TankName => tankName;
        public float MaxForwardSpeed => maxForwardSpeed;
        public float MaxBackwardsSpeed => maxBackwardsSpeed;

        public float EngineHP => engineHP;
        public float Drag => drag;

        public AnimationCurve EngineCurve => engineCurve;
    }
}
