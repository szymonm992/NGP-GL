using Frontend.Scripts.Components;
using Frontend.Scripts.Enums;
using GLShared.General.Enums;
using System.Collections.Generic;
using Zenject;

namespace Frontend.Scripts.Interfaces
{
    public interface IVehicleController : IInitializable
    {
        VehicleType VehicleType { get; }
        bool HasAnyWheels { get; }
        bool DoesGravityDamping { get; }
        IEnumerable<UTAxle> AllAxles { get; }
        IEnumerable<UTWheel> AllWheels { get; }
        float AbsoluteInputY { get; }
        float AbsoluteInputX { get; }
        float SignedInputY { get; }

        float CurrentSpeed { get; }
        float CurrentSpeedRatio { get; }
        float MaxForwardSpeed { get; }
        float MaxBackwardsSpeed { get; }
        bool IsUpsideDown { get; }
        ForceApplyPoint BrakesForceApplyPoint { get; }
        ForceApplyPoint AccelerationForceApplyPoint { get; }
        float VisualElementsMovementSpeed { get; }

        abstract float GetCurrentMaxSpeed();
        abstract void SetupRigidbody();
    }
}
