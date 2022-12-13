using GLShared.General.Interfaces;
using GLShared.General.Models;
using GLShared.Networking.Components;
using GLShared.Networking.Interfaces;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Frontend.Scripts.Components
{
    public class FrontendSyncManager : MonoBehaviour, ISyncManager
    {
        [Inject] private readonly IVehiclesDatabase vehicleDatabase;
        [Inject] private readonly PlayerSpawner playerSpawner;

        private readonly Dictionary<string, INetworkEntity> connectedPlayers = new Dictionary<string, INetworkEntity>();

        private int spanwedPlayersAmount = 0;

        public int SpawnedPlayersAmount => spanwedPlayersAmount;

        public void CreatePlayer(bool isLocal, string vehicleName, Vector3 spawnPosition, Quaternion spawnRotation)
        {
            var playerProperties = GetPlayerInitData(isLocal, vehicleName, spawnPosition, spawnRotation);
            var prefabEntity = playerProperties.PlayerContext.gameObject.GetComponent<PlayerEntity>();//this references only to prefab
            var playerEntity = playerSpawner.Spawn(prefabEntity, playerProperties);

            connectedPlayers.Add("localPlayer", playerEntity);
            spanwedPlayersAmount++;
        }

        private PlayerProperties GetPlayerInitData(bool isLocal, string vehicleName, Vector3 spawnPosition, Quaternion spawnRotation)
        {
            //TODO: handling check whether the player is local or not

            var vehicleData = vehicleDatabase.GetVehicleInfo(vehicleName);
            if(vehicleData != null)
            {
                return new PlayerProperties()
                {
                    PlayerContext = vehicleData.VehiclePrefab,
                    PlayerVehicleName = vehicleData.VehicleName,
                    IsLocal = isLocal,
                    SpawnPosition = spawnPosition,
                    SpawnRotation = spawnRotation,
                };
            }
            return null;
        }
    }
}
