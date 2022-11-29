using Frontend.Scripts.Interfaces;
using GLShared.General.Interfaces;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Frontend.Scripts.Components
{
    public class TempBotInputProvider : MonoBehaviour, IPlayerInputProvider
    {
        public float Vertical => 0;

        public float Horizontal => 0;

        public bool Brake => true;

        public float RawHorizontal => 0;

        public float RawVertical => 0;

        public float CombinedInput => 0;

        public float SignedVertical => 0;

        public float SignedHorizontal => 0;

        public float AbsoluteVertical => 0;

        public float AbsoluteHorizontal => 0;

        public float LastVerticalInput => 0;

        public bool SnipingKey => false;

        public bool TurretLockKey => false;
    }
}
