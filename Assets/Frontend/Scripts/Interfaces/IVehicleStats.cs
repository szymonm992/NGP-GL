using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Frontend.Scripts
{
    public interface IVehicleStats
    {
        #region MAIN

        public string TankName { get; }


        #endregion


        #region GENERAL PARAMETERS
        //general
        public float MaxForwardSpeed { get; }
        public float MaxBackwardsSpeed { get; }

        #endregion


        #region ENGINE
        //engine 
        public float EngineHP { get; }
        public float Drag { get; }

        public float SuspensionSpring { get; }
        public float SuspensionDamper { get; }

        public AnimationCurve EnginePowerCurve { get; }

        #endregion
    }
}
