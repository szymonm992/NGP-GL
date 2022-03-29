using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Frontend.Scripts.Models.VehicleSystem
{
    public class CarStats : VehicleStatsBase
    {

        [Header("Step up")]
        [SerializeField] private float stepUpForce;
        [SerializeField] private float stepUpRayLength;

        public float StepUpForce => stepUpForce;
        public float StepUpRayLength => stepUpRayLength;
    }
}
