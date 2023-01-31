using Frontend.Scripts.Extensions;
using Frontend.Scripts.Signals;
using GLShared.General.Models;
using GLShared.General.Signals;
using GLShared.Networking.Components;
using GLShared.Networking.Extensions;
using GLShared.Networking.Models;
using Sfs2X.Core;
using Sfs2X.Entities;
using Sfs2X.Entities.Data;
using System;
using UnityEngine;
using Zenject;

namespace Frontend.Scripts.Components
{
    public class FrontendSyncManager : SyncManagerBase
    {
        [Inject] private readonly ConnectionManager connectionManager;
        [Inject] private readonly TimeManager timeManager;

        [SerializeField] private float inputSendingPeriod = 0.01f;

        private float timeLastSendingInput;
        private PlayerEntity localPlayerEntity;

        public PlayerEntity LocalPlayerEntity => localPlayerEntity;

        public override void Initialize()
        {
            if (smartFox.IsInitialized && smartFox.Connection.IsConnected)
            {
                smartFox.Connection.AddEventListener(SFSEvent.EXTENSION_RESPONSE, OnExtensionResponse);
            }
        }

        protected override void CreatePlayer(string username, Vector3 spawnPosition, Vector3 spawnEulerAngles, out PlayerProperties playerProperties)
        {
            var user = smartFox.Connection.UserManager.GetUserByName(username);

            if (user == null)
            {
                playerProperties = null;
                Debug.LogError($"User '{username}' has not been found in User Manager");
                return;
            }

            base.CreatePlayer(username, spawnPosition, spawnEulerAngles, out playerProperties);

            if (user.IsItMe)
            {
                localPlayerEntity = connectedPlayers[user.Name];
            }
        }

        protected override PlayerProperties GetPlayerInitData(string username, string vehicleName,
            Vector3 spawnPosition, Vector3 spawnEulerAngles)
        {
            var vehicleData = vehicleDatabase.GetVehicleInfo(vehicleName);
            var user = smartFox.Connection.UserManager.GetUserByName(username);

            if (vehicleData != null)
            {
                return new PlayerProperties()
                {
                    PlayerContext = vehicleData.VehiclePrefab,
                    PlayerVehicleName = vehicleData.VehicleName,
                    IsLocal = user.IsItMe,
                    SpawnPosition = spawnPosition,
                    SpawnRotation = Quaternion.Euler(spawnEulerAngles.x, spawnEulerAngles.y, spawnEulerAngles.z),
                    Username = username,
                };
            }

            return null;
        }

        private void OnExtensionResponse(BaseEvent evt)
        {
            string cmd = (string)evt.Params["cmd"];
            ISFSObject responseData = (SFSObject)evt.Params["params"];

            try
            {
                if (cmd == "serverTime")
                {
                    long time = responseData.GetLong("t");
                    currentServerTime = Convert.ToDouble(time);
                    double ping = timeManager.GetAveragePingAndSync(currentServerTime);

                    signalBus.Fire(new ConnectionSignals.OnPingUpdate()
                    {
                        CurrentAveragePing = ping,
                    });
                }
                if (cmd == "sendGameStage")
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
                if (cmd == "playerSync")
                {
                    NetworkTransform newTransform = responseData.ToNetworkTransform();

                    if (connectedPlayers.ContainsKey(newTransform.Username))
                    {
                        connectedPlayers[newTransform.Username].ReceiveSyncPosition(newTransform);
                    }
                }
                if (cmd == "playerSpawned")
                {
                    var spawnData = responseData.ToSpawnData();
                    TryCreatePlayer(spawnData.Username, spawnData.SpawnPosition, spawnData.SpawnEulerAngles);

                }
                if (cmd == "battleTimer")
                {
                    int minutesLeft = responseData.GetInt("minutesLeft");
                    int secondsLeft = responseData.GetInt("secondsLeft");

                    signalBus.Fire(new PlayerSignals.OnBattleTimeChanged()
                    {
                        CurrentMinutesLeft = minutesLeft,
                        CurrentSecondsLeft = secondsLeft,
                    });
                }
            }
            catch (Exception exception)
            {
                Debug.Log(" Frontend Syncmanager exception handling response: " + exception.Message
                   + " >>>[AND TRACE IS]>>> " + exception.StackTrace);
            }
        }

        protected override void Update()
        {
            if(smartFox.IsInitialized)
            {
                smartFox.Connection.ProcessEvents();

                if(localPlayerEntity != null)
                {
                    SyncLocalPlayerInput();
                }
            }
        }

        private void SyncLocalPlayerInput()
        {
            if (timeLastSendingInput >= inputSendingPeriod)
            {
                connectionManager.SendUDPRequest(NetworkConsts.RPC_PLAYER_INPUTS, localPlayerEntity.Input.ToISFSOBject());
                timeLastSendingInput = 0;
                return;
            }

            timeLastSendingInput += Time.deltaTime;
        }
    }
}
