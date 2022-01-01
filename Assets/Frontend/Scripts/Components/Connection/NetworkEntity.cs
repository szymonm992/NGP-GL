using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Frontend.Scripts.Enums;

namespace Frontend.Scripts.Components
{
    public class NetworkEntity : MonoBehaviour, INetworkEntity
    {
        [SerializeField] private NetworkEntityType objectType;
        public NetworkEntityType EntityType => objectType;
    }
}
