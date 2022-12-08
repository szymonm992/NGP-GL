using Frontend.Scripts.Models;
using GLShared.General.Interfaces;
using GLShared.General.Models;
using GLShared.Networking.Components;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Frontend.Scripts.Components
{
    public class PlayerEntity : NetworkEntity
    {
        [SerializeField] private bool isLocalPlayer;
    
        private PlayerProperties playerProperties;

        public bool IsLocalPlayer => isLocalPlayer;
        public PlayerProperties PlayerProperties => playerProperties;

        [Inject]
        public void Construct(PlayerProperties properties)
        {
            UpdateProperties(properties);
            
        }

        public void UpdateProperties(PlayerProperties properties)
        {
            playerProperties = properties;

            transform.SetPositionAndRotation(playerProperties.SpawnPosition, playerProperties.SpawnRotation);
            isLocalPlayer = playerProperties.IsLocal;

            Debug.Log("0");
        }

        public class Factory : PlaceholderFactory<PlayerProperties, PlayerEntity>
        {

        }

    }
}
