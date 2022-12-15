using Automachine.Scripts.Components;
using Frontend.Scripts.Enums;
using Frontend.Scripts.Models;
using Frontend.Scripts.Signals;
using GLShared.General.Enums;
using GLShared.Networking.Components;
using Sfs2X;
using Sfs2X.Core;
using Sfs2X.Entities.Data;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.MemoryProfiler;
using UnityEngine;
using Zenject;

namespace Frontend.Scripts.Components.GameState
{
    public class WelcomeOnLobbyJoined : State<WelcomeStage>
    {
        [Inject] private readonly SmartFoxConnection smartFox;
        [Inject] private readonly ConnectionManager connectionManager;
        [Inject(Id = "welcomeCanvas")] private readonly RectTransform welcomeUi;
        [Inject(Id = "lobbyCanvas")] private readonly RectTransform lobbyCanvas;
        [Inject(Id = "connectedUsersAmount")] private readonly TextMeshProUGUI connectedUsersAmount;
        [Inject(Id = "gameVersion")] private readonly TextMeshProUGUI gameVersion;

        private float serverSettingsTimer = 0;

        public override void Initialize()
        {
            base.Initialize();
            signalBus.Subscribe<ConnectionSignals.OnServerSettingsResponse>(HandleServerSettings);
        }
        public override void StartState()
        {
            base.StartState();

            welcomeUi.gameObject.ToggleGameObjectIfActive(false);
            lobbyCanvas.gameObject.ToggleGameObjectIfActive(true);

            GetServerSettings();
        }

        public override void Tick()
        {
            base.Tick();
            if(isActive)
            {
                if(serverSettingsTimer < 15f)
                {
                    serverSettingsTimer += Time.deltaTime;
                }
                else
                {
                    GetServerSettings();
                    serverSettingsTimer = 0;
                }
            }
        }

        private void GetServerSettings()
        {
            var serverData = new SFSObject();
            connectionManager.SendRequest("getServerSettings", serverData);
        }

        private void HandleServerSettings(ConnectionSignals.OnServerSettingsResponse OnServerSettingsResponse)
        {
            var serverResponseData = OnServerSettingsResponse.ServerSettingsData;
            var dataGameVersion = serverResponseData.GetUtfString("gameVersion").ToString();
            var dataGameName = serverResponseData.GetUtfString("gameName").ToString();
            var dataPlayersConnected = serverResponseData.GetInt("playersConnected");

            if (dataGameVersion != Application.version)
            {
                smartFox.DisconnectError = "Your game version is inactual!";
                smartFox.Disconnect();
                return;
            }

            gameVersion.text = "Game version: "+ dataGameVersion;
            connectedUsersAmount.text = dataPlayersConnected.ToString();

            Debug.Log("Updated game settings for '"+ dataGameName + "' -> Game version: " + gameVersion);
        }
    }
}
