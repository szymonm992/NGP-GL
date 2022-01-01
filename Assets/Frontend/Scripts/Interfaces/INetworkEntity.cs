using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Frontend.Scripts.Enums;

namespace Frontend.Scripts.Components
{
    public interface INetworkEntity
    {
        public NetworkEntityType EntityType { get; }
    }
}
