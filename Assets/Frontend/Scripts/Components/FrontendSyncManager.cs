using Frontend.Scripts.Extensions;
using Frontend.Scripts.Signals;
using GLShared.General.Interfaces;
using GLShared.General.Models;
using GLShared.Networking.Components;
using GLShared.Networking.Interfaces;
using Sfs2X.Core;
using Sfs2X.Entities;
using Sfs2X.Entities.Data;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;
using Zenject;

namespace Frontend.Scripts.Components
{
    public class FrontendSyncManager : MonoBehaviour, ISyncManager
    {
        [Inject] private readonly SignalBus signalBus;
        [Inject] private readonly IVehiclesDatabase vehicleDatabase;
        [Inject] private readonly PlayerSpawner playerSpawner;
        [Inject] private readonly ConnectionManager connectionManager;
        [Inject] private readonly SmartFoxConnection smartFox;
        [Inject] private readonly TimeManager timeManager;

        private readonly Dictionary<string, INetworkEntity> connectedPlayers = new Dictionary<string, INetworkEntity>();

        private int spanwedPlayersAmount = 0;
        private double currentServerTime = 0;
        public int SpawnedPlayersAmount => spanwedPlayersAmount;

        public double CurrentServerTime => currentServerTime;

        public void Initialize()
        {
            if (smartFox.IsInitialized && smartFox.Connection.IsConnected)
            {
                smartFox.Connection.AddEventListener(SFSEvent.EXTENSION_RESPONSE, OnExtensionResponse);
            }
        }

        public void TryCreatePlayer(User user, Vector3 spawnPosition, Quaternion spawnRotation)
        {
            if (!connectedPlayers.ContainsKey(user.Name))
            {
                CreatePlayer(user, spawnPosition, spawnRotation);
            }
        }

        public void CreatePlayer(User user, Vector3 spawnPosition, Quaternion spawnRotation)
        {
            var vehicleName = user.GetVariable("playerVehicle").Value.ToString();
            var playerProperties = GetPlayerInitData(user, vehicleName, spawnPosition, spawnRotation);
            var prefabEntity = playerProperties.PlayerContext.gameObject.GetComponent<PlayerEntity>();//this references only to prefab
            var playerEntity = playerSpawner.Spawn(prefabEntity, playerProperties);

            connectedPlayers.Add(user.Name, playerEntity);
            spanwedPlayersAmount++;
        }

        private PlayerProperties GetPlayerInitData(User user, string vehicleName,
            Vector3 spawnPosition, Quaternion spawnRotation)
        {
            var vehicleData = vehicleDatabase.GetVehicleInfo(vehicleName);
            if (vehicleData != null)
            {
                return new PlayerProperties()
                {
                    PlayerContext = vehicleData.VehiclePrefab,
                    PlayerVehicleName = vehicleData.VehicleName,
                    IsLocal = user.IsItMe,
                    SpawnPosition = spawnPosition,
                    SpawnRotation = spawnRotation,
                    User = user,
                };
            }
            return null;
        }

        private void OnExtensionResponse(BaseEvent evt)
        {
            try
            {
                string cmd = (string)evt.Params["cmd"];
                ISFSObject responseData = (SFSObject)evt.Params["params"];
               
                if (cmd == "serverTime")
                {
                    long time = responseData.GetLong("t");
                    currentServerTime = Convert.ToDouble(time);
                    
                    var ping = timeManager.GetAveragePingAndSync(currentServerTime);

                    signalBus.Fire(new ConnectionSignals.OnPingUpdate()
                    {
                        CurrentAveragePing = ping,
                    });
                }
                if(cmd == "gameStage")
                {
                    int currentStage = responseData.GetInt("currentGameStage");
                    signalBus.Fire(new BattleSignals.OnGameStageUpdate()
                    {
                        CurrentGameStage = currentStage,
                    });
                }
                if (cmd == "gameStartCountdown")
                {
                    int currentCountdownValue = responseData.GetInt("currentCountdownValue");
                    signalBus.Fire(new BattleSignals.OnCounterUpdate()
                    {
                        CurrentValue = currentCountdownValue,
                    });
                }
                if (cmd == "playerSpawned")
                {
                   
                    var spawnData = responseData.ToSpawnData();
                    var user = smartFox.Connection.UserManager.GetUserByName(spawnData.Username);
                    if(user != null)
                    {
                        TryCreatePlayer(user, spawnData.SpawnPosition, spawnData.SpawnRotation);
                    }   
                    else
                    {
                        Debug.LogError("Player " + spawnData.Username + " has not been dfound in users manager");
                    }
                }

            }
            catch (Exception exception)
            {
                Debug.Log(" Frontend Syncmanager exception handling response: " + exception.Message
                   + " >>>[AND TRACE IS]>>> " + exception.StackTrace);
            }
        }

        private void Update()
        {
            if(smartFox.IsInitialized)
            {
                smartFox.Connection.ProcessEvents();
            }
        }
    }
}
