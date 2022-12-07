using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Frontend.Scripts.Interfaces
{
    public interface IPlayerEntity 
    {
        bool IsLocalPlayer { get; }
    }
}
