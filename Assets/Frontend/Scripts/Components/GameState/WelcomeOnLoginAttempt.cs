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
using static Frontend.Scripts.Signals.ConnectionSignals;
using Sfs2X.Requests;

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
                FinishLoginAttempt(false, message);
                return;
            }

            smartFox.Connection.Send(new LoginRequest(login, password, "GLServerGateway"));
        }

        private void FinishLoginAttempt(bool result, string message = "")
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
            //smartFox.Connection.AddEventListener(SFSEvent.ROOM_JOIN_ERROR, OnRoomJoinError);
            //smartFox.Connection.AddEventListener(SFSEvent.ROOM_JOIN, OnRoomJoin);

            smartFox.Connection.Connect(smartFox.HOST, smartFox.PORT);
        }

        private void OnConnectionAttemptResult(OnConnectionAttemptResult OnConnectionAttemptResult)
        {
            bool successfulCon = OnConnectionAttemptResult.SuccessfullyConnected;
            if (successfulCon)
            {
                CheckLogin();
            }
            else
            {
                FinishLoginAttempt(false, "Failed connecting to server!");
            }
        }
        
        public void OnLoginAttemptResult(OnLoginAttemptResult OnLoginAttemptResult)
        {
            FinishLoginAttempt(OnLoginAttemptResult.SuccessfullyLogin, OnLoginAttemptResult.LoginMessage);
        }
    }
}
