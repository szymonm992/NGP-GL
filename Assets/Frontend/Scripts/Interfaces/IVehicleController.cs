using Frontend.Scripts.Components;
using Zenject;

namespace Frontend.Scripts.Interfaces
{
    public interface IVehicleController : IInitializable
    {
        bool HasAnyWheels { get; }
        UTAxle[] AllAxles { get; }
        float CurrentSpeed { get; }


        float SignedInputY { get; }
        float AbsoluteInputY { get; }
        float AbsoluteInputX { get; }
        //void TurningLogic();

        //void MovementLogic();
    }
}
