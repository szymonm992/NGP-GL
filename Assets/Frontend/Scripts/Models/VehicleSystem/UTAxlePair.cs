using Frontend.Scripts.Components;
using UnityEngine;
using GLShared.General.Models;

namespace Frontend.Scripts.Models
{
    [System.Serializable]
    public class UTAxlePair : UTAxlePairBase
    {
        [System.Serializable]
        public struct DummyPair
        {
            public WheelDummy trackDummy;
            public float dummyOffsetY;
        }
    
        [SerializeField] private Transform tireModel;
        [SerializeField] private DummyPair dummyPair;

        private Transform visualPartOfTire;
        private Transform rotationalPartOfTire;

        public Transform TireModel => tireModel;
        public Transform RotationalPartOfTire => rotationalPartOfTire;
        public Transform VisualPartOfTire => visualPartOfTire;
        public DummyPair WheelDummyPair => dummyPair;
   
        public override void Initialize()
        {
            base.Initialize();
            visualPartOfTire = tireModel.GetChild(0);
            rotationalPartOfTire = visualPartOfTire.GetChild(0);
        }

    }
}
