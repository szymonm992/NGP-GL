using Frontend.Scripts.Models;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Frontend.Scripts.Interfaces
{
    public interface ICameraEventBroadcaster
    {
        
        event GameEvents.CameraBound OnCameraTargetBound;

        void NotifyCameraTargetBound(GameObjectContext context, Vector3 startingEulerAngles);

    }


}
