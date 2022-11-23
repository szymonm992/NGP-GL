using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Frontend.Scripts.Signals
{
    public class BattleSignals
    {
        public class CameraSignals
        {
            public struct OnCameraBound
            {
                public GameObjectContext context;
                public Vector3 startingEulerAngles;
            }

            public struct OnCameraZoomChanged
            {
                public bool zoomValue;
            }
        }
    }
}
