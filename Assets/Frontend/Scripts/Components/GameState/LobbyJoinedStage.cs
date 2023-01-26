using Automachine.Scripts.Components;
using Frontend.Scripts.Enums;
using Frontend.Scripts.Models;
using Frontend.Scripts.Signals;
using GLShared.General.Enums;
using GLShared.General.ScriptableObjects;
using GLShared.Networking.Components;
using Sfs2X;
using Sfs2X.Core;
using Sfs2X.Entities.Data;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Frontend.Scripts.Components.GameState
{
    public class LobbyJoinedStage : State<WelcomeStage>
    {
        [Inject] private readonly GameParameters gameParameters;
        [Inject] private readonly SmartFoxConnection smartFox;
        [Inject] private readonly ConnectionManager connectionManager;

        [Inject(Id = "welcomeCanvas")] private readonly RectTransform welcomeUi;
        [Inject(Id = "lobbyCanvas")] private readonly RectTransform lobbyCanvas;
        [Inject(Id = "connectedUsersAmount")] private readonly TextMeshProUGUI connectedUsersAmount;
        [Inject(Id = "joinBattleBtn")] private readonly Button joinBattleBtn;
        [Inject(Id = "gameVersion")] private readonly TextMeshProUGUI gameVersion;
        [Inject(Id = "canvasSearchingBattle")] private readonly RectTransform joinBattleCanvas;

        private float serverSettingsTimer = 0;
        private bool firstRun = true;

        public override void Initialize()
        {
            base.Initialize();
            signalBus.Subscribe<ConnectionSignals.OnServerSettingsResponse>(HandleServerSettings);
        }

        public override void StartState()
        {
            base.StartState();

            firstRun = true;

            GetServerSettings();
        }

        public override void Dispose()
        {
            base.Dispose();

            joinBattleCanvas.gameObject.ToggleGameObjectIfActive(false);
            lobbyCanvas.gameObject.ToggleGameObjectIfActive(false);
        }

        public override void Tick()
        {
            base.Tick();

            if (isActive)
            {
                if(serverSettingsTimer < gameParameters.ServerSettingsUpdateRate)
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
           
            gameVersion.text = $"Game version: {dataGameVersion}";
            connectedUsersAmount.text = dataPlayersConnected.ToString();

            if (firstRun)
            {
                welcomeUi.gameObject.ToggleGameObjectIfActive(false);
                lobbyCanvas.gameObject.ToggleGameObjectIfActive(true);
                joinBattleCanvas.gameObject.ToggleGameObjectIfActive(false);
                firstRun = false;
            }

            joinBattleBtn.interactable = true;
            Debug.Log($"Updated game settings for '{dataGameName}' -> Game version: {dataGameVersion}");
        }
    }
}
