using Frontend.Scripts.Components;
using GLShared.General.Models;
using GLShared.General.ScriptableObjects;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Frontend.Scripts.ScriptableObjects
{
    [CreateAssetMenu(fileName = "VehiclesDatabase", menuName = "UT/Databases/Frontend vehicles database")]
    public class FrontendVehicleDatabase : VehiclesDatabase
    {
        [SerializeField] private VehicleEntryInfo[] allVehicles;

        public override IEnumerable<VehicleEntryInfo> AllVehicles => allVehicles;
    }
}
