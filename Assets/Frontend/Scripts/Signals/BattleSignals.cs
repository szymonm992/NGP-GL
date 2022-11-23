using Frontend.Scripts.Interfaces;
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
            public class OnCameraBound
            {
                public GameObjectContext context;
                public Vector3 startingEulerAngles;
                public IPlayerInputProvider inputProvider;
            }

            public class OnCameraZoomChanged
            {
                public bool zoomValue;
            }
        }
    }
}
