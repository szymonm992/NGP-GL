using Frontend.Scripts.Components;


namespace Frontend.Scripts.Interfaces
{
    public interface IVehicleController
    {
        bool HasAnyWheels { get; }
        UTAxle[] AllAxles { get; }
        float CurrentSpeed { get; }

        //void TurningLogic();

        //void MovementLogic();
    }
}
