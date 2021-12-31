using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
using UnityEngine.Scripting;
using System.Linq;
using UnityEngine.UI;


using Sfs2X;
using Sfs2X.Util;
using Sfs2X.Core;
using Sfs2X.Requests;
using Sfs2X.Entities.Data;

namespace Frontend.Scripts.Components
{
    public class OnLoginState : GameStateEntity, IGameState
    {

        [Inject] private WelcomeManager manager;
        [Inject] private readonly SmartFoxConnection smartFoxConnection;
        [Inject] private readonly ConnectionManager connectionManager;
        [Inject] private readonly AsyncProcessor asyncProcessor;


        public override GameState ConnectedState { get; set; }

        private string loadedLogin;
        private string loadedPassword;

        private SmartFox sfs;

        public OnLoginState(GameState st)
        {
            ConnectedState = st;
        }

        public override void Start()
        {
            base.Start();
            ReadCredentials();
            StartLogin();
        }

        public void ReadCredentials()
        {
            this.loadedLogin = manager.AssociatedUI.GetElement("inp_login").ReturnAs<InputField>().text;
            this.loadedPassword = manager.AssociatedUI.GetElement("inp_pwd").ReturnAs<InputField>().text;
        }

        public override void Tick()
        {
            if (!IsActive)  return; 

            if (sfs != null)
            {
                sfs.ProcessEvents();
            }
           
        }

        private void StartLogin()
        {
            Debug.Log("Onlogin state started...");

            sfs = new SmartFox { ThreadSafeMode = true };
            sfs.AddEventListener(SFSEvent.CONNECTION, OnConnection);
            sfs.AddEventListener(SFSEvent.CONNECTION_LOST, OnConnectionLost);
            sfs.AddEventListener(SFSEvent.CONNECTION_RETRY, OnConnectionRetry);
            sfs.AddEventListener(SFSEvent.LOGIN, OnLogin);
            sfs.AddEventListener(SFSEvent.LOGIN_ERROR, OnLoginError);
            sfs.AddEventListener(SFSEvent.ROOM_JOIN_ERROR, OnRoomJoinError);
            sfs.AddEventListener(SFSEvent.ROOM_JOIN, OnRoomJoin);
            sfs.Connect(ConnectionManager.HOST, ConnectionManager.PORT);
            Debug.Log("Trying to connect...");
        }
        private void OnConnectionRetry(BaseEvent evt)
        {
            Debug.Log("Retrying connection...");
        }

        private void ResetSettings()
        {
            manager.AssociatedUI.ToggleUI(true);
            sfs.RemoveAllEventListeners();
        }

        private void OnConnection(BaseEvent evt)
        {
            if ((bool)evt.Params["success"])
            {
                smartFoxConnection.Connection = sfs;
                sfs.Send(new LoginRequest(loadedLogin, loadedPassword, "GoldLeague"));
            }
            else
            {
                ResetSettings();
                manager.AssociatedUI.DisplayError("Failed connecting to server!");
                gameStateManager.ChangeState(GameState.Welcome);
                this.Dispose();

            }
        }


        private void OnConnectionLost(BaseEvent evt)
        {
            string reason = (string)evt.Params["reason"];

            if (reason != ClientDisconnectionReason.MANUAL)
            {
                manager.AssociatedUI.DisplayError("Connection lost; the reason is: " + reason);
            }
            ResetSettings();
        }

        private void OnLogin(BaseEvent evt)
        {
            ISFSObject data = new SFSObject();
            data.PutText("RoomName", "Lobby");
            connectionManager.SendRoomJoinRequest("clientJoinLobby", data);
        }

        private void OnRoomJoinError(BaseEvent evt)
        {
            manager.AssociatedUI.ToggleUI(true);
            manager.AssociatedUI.DisplayError("Room join failed: " + (string)evt.Params["errorMessage"]);
        }

        private void OnLoginError(BaseEvent evt)
        {
            manager.AssociatedUI.ToggleUI(true);
            manager.AssociatedUI.DisplayError("Login failed: " + (string)evt.Params["errorMessage"]);
        }

        private void OnRoomJoin(BaseEvent evt)
        {
            ResetSettings();
            gameStateManager.ChangeState(GameState.Lobby);
        }
    }
}
