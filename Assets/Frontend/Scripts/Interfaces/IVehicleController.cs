using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Frontend.Scripts.Interfaces
{
    public interface IVehicleController
    {
        bool HasAnyWheels { get; }
        ICustomWheel[] AllWheels { get; }
        float CurrentSpeed { get; }

        //void TurningLogic();

        //void MovementLogic();
    }
}
