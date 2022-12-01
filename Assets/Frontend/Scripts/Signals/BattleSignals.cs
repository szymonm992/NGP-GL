using Frontend.Scripts.Interfaces;
using GLShared.General.Interfaces;
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
                public GameObjectContext context { get; set; }
                public Vector3 startingEulerAngles { get; set; }
                public IPlayerInputProvider inputProvider { get; set; }
            }

            public class OnCameraModeChanged
            {
                public GameObject playerObject { get; set; }
                public bool IsSniping { get; set; }
            }
        }

        public class PlayerSignals
        {
            public class OnLocalPlayerInitialized
            {

            }
        }
    }
}
