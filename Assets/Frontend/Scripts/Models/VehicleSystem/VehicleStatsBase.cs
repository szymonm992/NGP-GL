using GLShared.General.Enums;
using GLShared.General.Interfaces;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Frontend.Scripts.Models
{
    public abstract class VehicleStatsBase : MonoBehaviour, IVehicleStats
    {
        [Header("General")]
        [SerializeField] protected string vehicleName;

        [Header("Movement")]
        [SerializeField] protected float mass;
        [SerializeField] protected float drag;
        [SerializeField] protected float angularDrag;

        [Header("Combat")]
        [SerializeField] protected float gunDepression;
        [SerializeField] protected float gunElevation;

        public string VehicleName => vehicleName;

        #region MOVEMENT PARAMETERS
        //general
        public float Mass => mass;
        public float Drag => drag;
        public float AngularDrag => angularDrag;
        #endregion


        #region COMBAT

        public float GunDepression => gunDepression;
        public float GunElevation => gunElevation;
        #endregion
    }
}
