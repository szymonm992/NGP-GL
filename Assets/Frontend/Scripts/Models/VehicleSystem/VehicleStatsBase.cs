using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Frontend.Scripts.Models.VehicleSystem
{
    public abstract class VehicleStatsBase : MonoBehaviour
    {
        [Header("General")]
        [SerializeField] protected string vehicleName;

        [Header("Movement")]
        [SerializeField] protected float maxForwardSpeed;
        [SerializeField] protected float maxBackwardsSpeed;
        [SerializeField] protected float turnAngle;

        [Header("Engine")]
        [SerializeField] protected float enginePower;
        [SerializeField] protected float brakePower;
        [SerializeField] protected float turnPower;
        [SerializeField] protected float frictionPower;
        [SerializeField] protected float drag;



        [Header("Curves")]
        [SerializeField] protected AnimationCurve engineCurve;
        [SerializeField] protected AnimationCurve turningCurve;
        [SerializeField] protected AnimationCurve speedCurve;
        [SerializeField] protected AnimationCurve frictionCurve;

   

        #region MAIN
        //main
        public string VehicleName => vehicleName;
        #endregion

        #region MOVEMENT PARAMETERS
        //general

        public float MaxForwardSpeed => maxForwardSpeed;
        public float MaxBackwardsSpeed => maxBackwardsSpeed;
        public float TurnAngle => turnAngle;
        #endregion


        #region ENGINE

        public float EnginePower => enginePower;
        public float BrakePower => brakePower;
        public float TurnPower => turnPower;
        public float FrictionPower => frictionPower;
        public float Drag => drag;
        #endregion

        #region CURVES
        public AnimationCurve EngineCurve => engineCurve;
        public AnimationCurve TurningCurve => turningCurve;
        public AnimationCurve SpeedCurve => speedCurve;
        public AnimationCurve FrictionCurve => frictionCurve;

        #endregion
    }
}
