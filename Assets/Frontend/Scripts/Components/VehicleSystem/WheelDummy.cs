using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Frontend.Scripts.Components
{
    public class WheelDummy : MonoBehaviour, IInitializable
    {
        [SerializeField] private Transform upwardDummy;
        [SerializeField] private float upwardDummyOffset = 0;

        public float UpwardDummyOffset => upwardDummyOffset;

        public Transform UpwardDummy => upwardDummy;
        public Transform Holder { get; private set; }
        public Transform UpwardDummyHolder { get; private set; }

        public void Initialize()
        {
            Holder = transform.GetChild(0);
            UpwardDummyHolder = upwardDummy.GetChild(0);
        }
    }
}
