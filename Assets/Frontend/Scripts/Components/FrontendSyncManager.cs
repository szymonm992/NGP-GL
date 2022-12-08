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
            var playerInitData = GetPlayerInitData(vehicleName, spawnPosition, spawnRotation);
            var playerProperties = playerInitData.Item2;
            var playerContext = CreatePlayerContext(playerInitData.Item1, playerProperties);

            connectedPlayers.Add("localPlayer", playerContext.Container.Resolve<INetworkEntity>());

            signalBus.Fire(new PlayerSignals.OnPlayerSpawned()
            {
                PlayerProperties = playerProperties,
            });

            if(playerProperties.IsLocal)
            {
                if (playerProperties.IsLocal)
                {
                    signalBus.Fire(new BattleSignals.CameraSignals.OnCameraBound()
                    {
                        PlayerContext = playerProperties.PlayerContext,
                        StartingEulerAngles = playerProperties.PlayerContext.transform.eulerAngles,
                        InputProvider = playerProperties.PlayerContext.Container.Resolve<IPlayerInputProvider>()
                    });
                }
            }
        }

        private GameObjectContext CreatePlayerContext(GameObject prefab, PlayerProperties properties)
        {
            GameObject playerObject = Instantiate(prefab, properties.SpawnPosition, properties.SpawnRotation);
            var playerContext = playerObject.GetComponent<GameObjectContext>();
            playerObject.name = properties.PlayerVehicleName;
            return playerContext;
        }

        private (GameObject, PlayerProperties) GetPlayerInitData(string vehicleName, Vector3 spawnPosition, Quaternion spawnRotation)
        {
            //TODO: handling check whether the player is local or not

            (GameObject, PlayerProperties) retVal = default;
            var vehicleData = vehicleDatabase.GetVehicleInfo(vehicleName);
            if(vehicleData != null)
            {
                retVal = (vehicleData.VehiclePrefab, new PlayerProperties()
                {
                    PlayerContext = vehicleData.VehiclePrefab.GetComponent<GameObjectContext>(),
                    PlayerVehicleName = vehicleData.VehicleName,
                    IsLocal = true,
                    SpawnPosition = spawnPosition,
                    SpawnRotation = spawnRotation,
                });
            }
            return retVal;
        }
    }
}
