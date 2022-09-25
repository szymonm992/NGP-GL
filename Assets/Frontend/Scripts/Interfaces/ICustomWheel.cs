using Frontend.Scripts.Enums;
using UnityEngine;

namespace Frontend.Scripts.Interfaces
{
    public interface ICustomWheel
    {
        float SteerAngle { get; set; }
        bool CanSteer { get; }
        bool CanDrive { get; }
        DriveAxisSite WheelAxis { get; }
    }
}
