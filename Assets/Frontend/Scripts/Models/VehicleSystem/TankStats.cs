using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Frontend.Scripts.Models.VehicleSystem
{
    public class TankStats : MonoBehaviour, IVehicleStats
    {
        [SerializeField] private string tankName;

        [Header("Movement")]
        [SerializeField] private float maxForwardSpeed;
        [SerializeField] private float maxBackwardsSpeed;

        [Header("Engine")]
        [SerializeField] private float engineHP;
        [SerializeField] private float drag;

        [Header("Suspension")]
        [SerializeField] private float suspensionSpring;
        [SerializeField] private float suspensionDamper;
        
        


        [Header("Curves")]
        [SerializeField] private AnimationCurve engineCurve;

        #region MAIN
        //main
        public string TankName => tankName;
        #endregion

        #region GENERAL PARAMETERS
        //general

        public float MaxForwardSpeed => maxForwardSpeed;
        public float MaxBackwardsSpeed => maxBackwardsSpeed;
        #endregion


        #region ENGINE

        public float EngineHP => engineHP;
        public float Drag => drag;

        public AnimationCurve EnginePowerCurve => engineCurve;

        public float SuspensionSpring => suspensionSpring;

        public float SuspensionDamper =>  suspensionDamper;

        #endregion
    }
}
