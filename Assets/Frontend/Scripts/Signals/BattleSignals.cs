using Frontend.Scripts.Interfaces;
using Frontend.Scripts.Models;
using GLShared.General.Interfaces;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Frontend.Scripts.Signals
{
    public class BattleSignals
    {
        public class OnGameStageUpdate
        {
            public int CurrentGameStage { get; set; }
        }

        public class OnCounterUpdate
        {
            public int CurrentValue { get; set; }
        }

        public class CameraSignals
        {
            public class OnCameraBound
            {
                public GameObjectContext PlayerContext { get; set; }
                public Vector3 StartingEulerAngles { get; set; }
                public IPlayerInputProvider InputProvider { get; set; }
                public float GunDepression { get; set; }
                public float GunElevation { get; set; }
            }

            public class OnCameraModeChanged
            {
                public GameObject PlayerObject { get; set; }
                public bool IsSniping { get; set; }
            }
        }
    }
}
