using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Frontend.Scripts.Signals
{
    public class BattleSignals
    {
        public class OnCameraBound
        {
            public GameObjectContext context;
            public Vector3 startingEulerAngles;
        }
    }
    

}
