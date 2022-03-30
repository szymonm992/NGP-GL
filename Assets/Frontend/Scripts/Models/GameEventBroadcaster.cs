using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Frontend.Scripts.Interfaces;
using Zenject;

namespace Frontend.Scripts.Models
{
    public class GameEventBroadcaster : ICameraEventBroadcaster
    {
        public event GameEvents.CameraBound OnCameraTargetBound;

        public void NotifyCameraTargetBound(GameObjectContext context, Vector3 startingEulerAngles)
        {
            OnCameraTargetBound?.Invoke(context, startingEulerAngles);
        }
    }
}
