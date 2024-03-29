using Frontend.Scripts.Extensions;
using Frontend.Scripts.Signals;
using GLShared.General.Enums;
using GLShared.General.Models;
using GLShared.General.Signals;
using GLShared.Networking.Components;
using GLShared.Networking.Extensions;
using GLShared.Networking.Models;
using Sfs2X.Core;
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
        private PlayerInput lastReceivedOwnInput;
        private PlayerEntity localPlayerEntity;

        public PlayerEntity LocalPlayerEntity => localPlayerEntity;

        public override void Initialize()
        {
            if (smartFox.IsInitialized && smartFox.Connection.IsConnected)
            {
                smartFox.Connection.AddEventListener(SFSEvent.EXTENSION_RESPONSE, OnExtensionResponse);
            }
        }

        // This is responsible for sync of inputs of remote clients (other players in room).
        public override void SyncInputs(PlayerInput remotePlayerInputs)
        {
            if (connectedPlayers.ContainsKey(remotePlayerInputs.Username))
            {
                var player = connectedPlayers[remotePlayerInputs.Username];

                if (!player.IsLocalPlayer)
                {
                    base.SyncInputs(remotePlayerInputs);

                    connectedPlayers[remotePlayerInputs.Username].InputProvider.SetInput(remotePlayerInputs);
                }
                else
                {
                    lastReceivedOwnInput = remotePlayerInputs;
                }
            }
        }

        protected override void CreatePlayer(string username, Team team, Vector3 spawnPosition, Vector3 spawnEulerAngles, out PlayerProperties playerProperties)
        {
            var user = smartFox.Connection.UserManager.GetUserByName(username);

            if (user == null)
            {
                playerProperties = null;
                Debug.LogError($"User '{username}' has not been found in User Manager");
                return;
            }

            base.CreatePlayer(username, team, spawnPosition, spawnEulerAngles, out playerProperties);

            if (user.IsItMe)
            {
                localPlayerEntity = connectedPlayers[user.Name];
            }
        }

        protected override void CreateShell(string username, string databaseId, int sceneId, Vector3 spawnPosition, Vector3 spawnEulerAngles,
            (Vector3, float) targetingProperties, out ShellProperties shellProperties)
        {
            base.CreateShell(username, databaseId, sceneId, spawnPosition, spawnEulerAngles, targetingProperties, out shellProperties);

            signalBus.Fire(new ShellSignals.OnShellSpawned()
            {
                ShellProperties = shellProperties,
            });
        }

        protected override PlayerProperties GetPlayerInitData(string username, Team team, string vehicleName,
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
                    Team = team.ToString(),
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
                    var newTransform = responseData.ToNetworkTransform();

                    if (connectedPlayers.ContainsKey(newTransform.Identifier))
                    {
                        connectedPlayers[newTransform.Identifier].ReceiveSyncPosition(newTransform);
                    }
                }
                if (cmd == "playerSpawned")
                {
                    var spawnData = responseData.ToPlayerSpawnData();
                    TryCreatePlayer(spawnData.Username, Enum.Parse<Team>(spawnData.Team), spawnData.SpawnPosition, spawnData.SpawnEulerAngles);
                }
                if (cmd == "shellSpawned")
                {
                    var spawnData = responseData.ToShellSpawnData();
                    TryCreateShell(spawnData.OwnerUsername, spawnData.DatabaseIdentifier, spawnData.SceneId, spawnData.SpawnPosition, spawnData.SpawnEulerAngles, new(spawnData.TargetingPosition, 0f));
                }
                if (cmd == "shellDestroyed")
                {
                    int sceneId = responseData.GetInt("id");
                    TryDestroyingShell(sceneId);
                }
                if (cmd == "shellSync")
                {
                    var newShellTransform = responseData.ToNetworkShellTransform();
                    int sceneId = int.Parse(newShellTransform.Identifier);

                    if (shells.ContainsKey(sceneId))
                    {
                        shells[sceneId].ReceiveSyncPosition(newShellTransform);
                    }
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
                if (cmd == "userVars")
                {
                    var remotePlayerInputs = responseData.ToRemotePlayerInput();
                    SyncInputs(remotePlayerInputs);
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
            // Sending our own inputs until server confirms it got them by sending them back.
            var currentInputs = localPlayerEntity.Input;
            if (timeLastSendingInput >= inputSendingPeriod && currentInputs != lastReceivedOwnInput)
            {
                connectionManager.SendUDPRequest(NetworkConsts.RPC_PLAYER_INPUTS, currentInputs.ToISFSOBject());
                timeLastSendingInput = 0;
                return;
            }

            timeLastSendingInput += Time.deltaTime;
        }
    }
}
