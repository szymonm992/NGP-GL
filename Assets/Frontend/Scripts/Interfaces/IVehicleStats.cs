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

        public float EngineHP { get; }
        public float Drag { get; }

        public AnimationCurve EngineCurve { get; }
    }
}
