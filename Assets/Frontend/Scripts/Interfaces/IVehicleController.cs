using Frontend.Scripts.Components;
using Frontend.Scripts.Enums;
using System.Collections.Generic;
using Zenject;

namespace Frontend.Scripts.Interfaces
{
    public interface IVehicleController : IInitializable
    {
        VehicleType VehicleType { get; }
        bool HasAnyWheels { get; }
        IEnumerable<UTAxle> AllAxles { get; }
        float CurrentSpeed { get; }
        float AbsoluteInputY { get; }

        float MaxForwardSpeed { get; }
        float MaxBackwardsSpeed { get; }

        abstract float GetCurrentMaxSpeed();
        void SetupRigidbody();
    }
}
