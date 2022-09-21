using Frontend.Scripts.Enums;
using Frontend.Scripts.Models;
using System.Collections.Generic;
using System.Linq;
using UInc.Core.Utilities;
using UnityEngine;

namespace Frontend.Scripts.Components
{
    public class UTAxle : MonoBehaviour
    {
        [SerializeField] private UTAxlePair[] wheelPairs;
        [SerializeField] private bool canDrive;
        [SerializeField] private bool canSteer;

        public UTAxlePair[] WheelPairs => wheelPairs;
        public bool CanDrive => canDrive;
        public bool CanSteer => canSteer;

        public IEnumerable<UTWheel> GetGroundedWheels()
        {
            return wheelPairs.Where(pair => pair.Wheel.IsGrounded == true).Select(pair => pair.Wheel).ToArray();
        }

        public void SetSteerAngle(float angleLeftAxis, float angleRightAxis)
        {
            foreach(var pair in wheelPairs)
            {
                pair.Wheel.SteerAngle = pair.Axis == DriveAxisSite.Left ? angleLeftAxis : angleRightAxis;
            }
        }
    }
}
