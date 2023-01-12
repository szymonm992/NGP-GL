using Frontend.Scripts.Extensions;
using Frontend.Scripts.Signals;
using GLShared.General.Models;
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

        [SerializeField] private float inputSendingPeriod = 0.03f;

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

        protected override void CreatePlayer(User user, Vector3 spawnPosition, Vector3 spawnEulerAngles, out PlayerProperties playerProperties)
        {
            base.CreatePlayer(user, spawnPosition, spawnEulerAngles, out playerProperties);
            if (user.IsItMe)
            {
                localPlayerEntity = connectedPlayers[user.Name];
            }
        }

        protected override PlayerProperties GetPlayerInitData(User user, string vehicleName,
            Vector3 spawnPosition, Vector3 spawnEulerAngles)
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
                    SpawnRotation = Quaternion.Euler(spawnEulerAngles.x, spawnEulerAngles.y, spawnEulerAngles.z),
                    User = user,
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

                    var ping = timeManager.GetAveragePingAndSync(currentServerTime);

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
                    var user = smartFox.Connection.UserManager.GetUserByName(spawnData.Username);
                    if (user != null)
                    {
                        TryCreatePlayer(user, spawnData.SpawnPosition, spawnData.SpawnEulerAngles);
                    }
                    else
                    {
                        Debug.LogError("Player " + spawnData.Username + " has not been found in users manager");
                    }
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
                connectionManager.SendUDPRequest("inbattle.playerInputs", localPlayerEntity.Input.ToISFSOBject());
                timeLastSendingInput = 0;
                return;
            }
            timeLastSendingInput += Time.deltaTime;
        }
    }
}
