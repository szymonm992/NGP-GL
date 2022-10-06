using Frontend.Scripts.Components;
using System.Collections.Generic;
using Zenject;

namespace Frontend.Scripts.Interfaces
{
    public interface IVehicleController : IInitializable
    {
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
