using Frontend.Scripts.ScriptableObjects;
using Frontend.Scripts.Signals;
using GLShared.General.Interfaces;
using GLShared.General.Models;
using GLShared.General.Signals;
using GLShared.Networking.Interfaces;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;
using static GLShared.General.Signals.PlayerSignals;

namespace Frontend.Scripts.Components
{
    public class FrontendSyncManager : MonoBehaviour, ISyncManager
    {
        [Inject] private readonly SignalBus signalBus;
        [Inject] private readonly IVehiclesDatabase vehicleDatabase;

        private readonly Dictionary<string, INetworkEntity> connectedPlayers = new Dictionary<string, INetworkEntity>();

        public void SpawnPlayer(string vehicleName, Vector3 spawnPosition, Quaternion spawnRotation)
        {
            var playerProperties = GetPlayerInitData(vehicleName, spawnPosition, spawnRotation);
            var playerContext = CreatePlayerContext(playerProperties);
            var playerEntity = playerContext.gameObject.GetComponent<PlayerEntity>();

            playerEntity.UpdateProperties(playerProperties);

            connectedPlayers.Add("localPlayer", playerEntity);
            signalBus.Fire(new PlayerSignals.OnPlayerSpawned()
            {
                PlayerProperties = playerProperties,
            });
        }

        private GameObjectContext CreatePlayerContext(PlayerProperties properties)
        {
            GameObjectContext playerObject = Instantiate(properties.PlayerContext, properties.SpawnPosition, properties.SpawnRotation);
            playerObject.name = properties.PlayerVehicleName;
            return playerObject;
        }

        private PlayerProperties GetPlayerInitData(string vehicleName, Vector3 spawnPosition, Quaternion spawnRotation)
        {
            //TODO: handling check whether the player is local or not

            var vehicleData = vehicleDatabase.GetVehicleInfo(vehicleName);
            if(vehicleData != null)
            {
                return new PlayerProperties()
                {
                    PlayerContext = vehicleData.VehiclePrefab,
                    PlayerVehicleName = vehicleData.VehicleName,
                    IsLocal = true,
                    SpawnPosition = spawnPosition,
                    SpawnRotation = spawnRotation,
                };
            }
            return null;
        }
    }
}
