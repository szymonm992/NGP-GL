using Automachine.Scripts.Components;
using Frontend.Scripts.Enums;
using GLShared.General.Enums;
using GLShared.Networking.Components;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Frontend.Scripts.Components.GameState
{
    public class WelcomeOnLoginAttempt : State<WelcomeStage>
    {
        [Inject(Id = "loginBtn")] private readonly Button loginBtn;
        [Inject(Id = "loginText")] private readonly InputField loginField;
        [Inject(Id = "passwordText")] private readonly InputField passwordField;
        [Inject(Id = "errorLabel")] private TextMeshProUGUI errorLabel;
        [Inject] private readonly FormValidator formValidator;

        private string login;
        private string password;

        private bool isTryingToLogin;

        public string Login => login;
        public string Password => password;

        public bool TriedToLogin { get; set; }
        public bool LoginResult { get; set; }
        public bool IsTryingToLogin => isTryingToLogin;

        public override void StartState()
        {
            base.StartState();
            CheckLogin();
        }

        public void TryLogin()
        {
            if (isTryingToLogin)
            {
                return;
            }

            isTryingToLogin = true;
        }

        public void CheckLogin()
        {
            loginBtn.interactable = false;
            loginField.interactable = false;
            passwordField.interactable = false;

            login = loginField.text;
            var loginValidation = formValidator.IsLoginValid(login);
            string message = string.Empty;
            if (!loginValidation.Item1)
            {
                message = loginValidation.Item2;
            }

            password = passwordField.text;
            var passwordValidation = formValidator.IsPasswordValid(password);
            if (!passwordValidation.Item1)
            {
                message = passwordValidation.Item2;
            }

            bool loginResult = (loginValidation.Item1 && passwordValidation.Item1);
            FinishLoginAttempt(loginResult, message);
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

        private void DisplayError(string errorCaption)
        {
            if(errorCaption != string.Empty)
            {
                LoginResult = false;
            }
            errorLabel.text = errorCaption;
        }

    }
}
