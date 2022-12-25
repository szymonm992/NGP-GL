using Automachine.Scripts.Components;
using Frontend.Scripts.Enums;
using GLShared.General.Enums;
using GLShared.Networking.Components;
using Sfs2X.Core;
using Sfs2X;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using Frontend.Scripts.Signals;
using Sfs2X.Requests;
using Sfs2X.Entities.Data;
using Sfs2X.Util;

namespace Frontend.Scripts.Components.GameState
{
    public class WelcomeOnLoginAttempt : State<WelcomeStage>
    {
        [Inject(Id = "loginBtn")] private readonly Button loginBtn;
        [Inject(Id = "loginText")] private readonly InputField loginField;
        [Inject(Id = "passwordText")] private readonly InputField passwordField;
        [Inject(Id = "errorLabel")] private TextMeshProUGUI errorLabel;

        [Inject] private readonly FormValidator formValidator;
        [Inject] private readonly SmartFoxConnection smartFox;
        [Inject] private readonly ConnectionManager connectionManager;

        private string login;
        private string password;

        private bool isTryingToLogin;

        public string Login => login;
        public string Password => password;

        public bool TriedToLogin { get; set; }
        public bool LoginResult { get; set; }
        public bool IsTryingToLogin => isTryingToLogin;

        public override void Initialize()
        {
            base.Initialize();
            signalBus.Subscribe<ConnectionSignals.OnConnectionAttemptResult>(OnConnectionAttemptResult);
            signalBus.Subscribe<ConnectionSignals.OnLoginAttemptResult>(OnLoginAttemptResult);
            signalBus.Subscribe<ConnectionSignals.OnLobbyJoinAttemptResult>(OnLobbyJoinResult);
        }

        public override void StartState()
        {
            base.StartState();
            ConnectToServer();
        }

        public void DisplayError(string errorCaption)
        {
            if (errorCaption != string.Empty)
            {
                LoginResult = false;
            }
            errorLabel.text = errorCaption;
        }

        public void TryLogin()
        {
            if (isTryingToLogin)
            {
                return;
            }

            isTryingToLogin = true;

            loginBtn.interactable = false;
            loginField.interactable = false;
            passwordField.interactable = false;
        }

        public void CheckLogin()
        {
            login = loginField.text;
            var loginValidation = formValidator.IsLoginValid(login);
            string message = string.Empty;
            if (!loginValidation.Item1)
            {
                message = loginValidation.Item2;
            }

            password = passwordField.text;
            var passwordValidation = formValidator.IsPasswordValid(password);
            if (loginValidation.Item1 && !passwordValidation.Item1)
            {
                message = passwordValidation.Item2;
            }

            bool loginResult = (loginValidation.Item1 && passwordValidation.Item1);

            if(!loginResult)
            {
                FinishGetInLobbyAttempt(false, message);
                return;
            }

            smartFox.Connection.Send(new LoginRequest(login, password, "GLServerGateway"));
        }

        private void FinishGetInLobbyAttempt(bool result, string message = "")
        {
            LoginResult = result;

            loginField.interactable = true;
            passwordField.interactable = true;
            loginBtn.interactable = true;

            if (!result)
            {
                DisplayError(message);
            }
            else
            {
                DisplayError(string.Empty);
            }

            TriedToLogin = true;
            isTryingToLogin = false;
        }

        public void ConnectToServer()
        {
            smartFox.Connection = new SmartFox()
            {
                ThreadSafeMode = true,  
                
            };

            smartFox.Connection.AddEventListener(SFSEvent.CONNECTION, connectionManager.OnConnection);
            smartFox.Connection.AddEventListener(SFSEvent.CONNECTION_LOST, connectionManager.OnConnectionLost);
            smartFox.Connection.AddEventListener(SFSEvent.LOGIN, connectionManager.OnLogin);
            smartFox.Connection.AddEventListener(SFSEvent.LOGIN_ERROR, connectionManager.OnLoginError);
            smartFox.Connection.AddEventListener(SFSEvent.ROOM_JOIN_ERROR, connectionManager.OnRoomJoinError);
            smartFox.Connection.AddEventListener(SFSEvent.ROOM_JOIN, connectionManager.OnRoomJoin);
            smartFox.Connection.AddEventListener(SFSEvent.EXTENSION_RESPONSE, connectionManager.OnExtensionResponse);
            smartFox.Connection.AddEventListener(SFSEvent.LOGOUT, connectionManager.OnLogout);

            smartFox.Connection.Connect(smartFox.HOST, smartFox.PORT);
        }

        private void OnConnectionAttemptResult(ConnectionSignals.OnConnectionAttemptResult OnConnectionAttemptResult)
        {
            bool successfulCon = OnConnectionAttemptResult.SuccessfullyConnected;
            if (successfulCon)
            {
                CheckLogin();
            }
            else
            {
                FinishGetInLobbyAttempt(false, "Failed connecting to server!");
            }
        }
        
        public void OnLoginAttemptResult(ConnectionSignals.OnLoginAttemptResult OnLoginAttemptResult)
        {
            if(!OnLoginAttemptResult.SuccessfullyLogin)
            {
                FinishGetInLobbyAttempt(false, OnLoginAttemptResult.LoginMessage);
            }
            else
            {
                SendLobbyJoinRequest();
            }
        }

        public void OnLobbyJoinResult(ConnectionSignals.OnLobbyJoinAttemptResult OnLobbyJoin)
        {
            FinishGetInLobbyAttempt(OnLobbyJoin.SuccessfullyJoinedLobby, OnLobbyJoin.LobbyJoinMessage);
        }

        public void SendLobbyJoinRequest()
        {
            ISFSObject data = new SFSObject();
            data.PutText("roomName", "Lobby");
            connectionManager.SendRoomJoinRequest("clientJoinLobby", data);
        }
    }
}
