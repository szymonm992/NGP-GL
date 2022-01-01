using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Frontend.Scripts
{
    public interface IVehicleStats
    {
        public string TankName { get; }

        public float MaxForwardSpeed { get; }
        public float MaxBackwardsSpeed { get; }
    }
}
