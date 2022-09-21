using Frontend.Scripts.Components;
using Frontend.Scripts.Enums;
using UnityEngine;

namespace Frontend.Scripts.Models
{
    [System.Serializable]
    public class UTAxlePair
    {
        [SerializeField] private UTWheel wheel;
        [SerializeField] private DriveAxisSite axis;

        public UTWheel Wheel => wheel;
        public DriveAxisSite Axis => axis;
    }
}
