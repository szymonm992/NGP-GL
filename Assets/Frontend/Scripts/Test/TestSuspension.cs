using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Frontend.Scripts.Components
{
    public class TestSuspension : MonoBehaviour
    {
        [SerializeField] private TestWheel[] allWheels;

        public TestWheel[] AllWheels => allWheels;
    }
}
