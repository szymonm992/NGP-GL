using GLShared.Networking.Components;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Frontend.Scripts.Components
{
    public class PlayerEntity : NetworkEntity
    {
        [SerializeField] private bool isLocalPlayer;
        public bool IsLocalPlayer => isLocalPlayer;
    }
}
