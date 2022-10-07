using Frontend.Scripts.Components;
using Frontend.Scripts.Enums;
using GLShared.General.Enums;
using UnityEngine;
using Zenject;

namespace Frontend.Scripts.Models
{
    [System.Serializable]
    public class UTAxlePair
    {
        [SerializeField] private UTWheel wheel;
        [SerializeField] private DriveAxisSite axis;
        [SerializeField] private Transform tireModel;

        private Transform visualPartOfTire;
        private Transform rotationalPartOfTire;
        public DriveAxisSite Axis => axis;
        public Transform TireModel => tireModel;
        public UTWheel Wheel => wheel;
        public Transform RotationalPartOfTire => rotationalPartOfTire;
        public Transform VisualPartOfTire => visualPartOfTire;

        public void Initialize()
        {
            visualPartOfTire = tireModel.GetChild(0);
            rotationalPartOfTire = visualPartOfTire.GetChild(0);
        }

    }
}
