using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Frontend.Scripts.Models.VehicleSystem
{
    public class CarStats : VehicleStatsBase
    {

        [Header("Step up")]
        [SerializeField] private float stepUpMultiplier=1440f;
        [SerializeField] private float stepUpRayLength;

        public float StepUpMultiplier => stepUpMultiplier;
        public float StepUpRayLength => stepUpRayLength;
    }
}
