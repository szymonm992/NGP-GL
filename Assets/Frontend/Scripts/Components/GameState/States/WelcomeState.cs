using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
using UnityEngine.Scripting;
using System.Linq;
using UnityEngine.UI;

namespace Frontend.Scripts.Components
{
    public class WelcomeState : GameStateEntity, IGameState
    {
       
        [Inject] private readonly WelcomeManager manager;
        public GameState ConnectedState { get; set; }


        private delegate void DisplayErrorDelegate(string content);
        public WelcomeState(GameState st)
        {
            ConnectedState = st;
        }
        
        public void Start() => SubscribeEvents();

        public void TryLogin()
        {
            string login = manager.AssociatedUI.GetElement("inp_login").UI_element.ReturnAs<InputField>().text;
            string password = manager.AssociatedUI.GetElement("inp_pwd").UI_element.ReturnAs<InputField>().text;
            if (IsLoginFormValid(login, password, manager.AssociatedUI.DisplayError))
            {
                Debug.Log("Valid ones");
                manager.AssociatedUI.ToggleUI(false);
            }
        }
        private bool IsLoginFormValid(string login, string password, DisplayErrorDelegate onInvalidCallback = null)
        {
            string error = "";

            if (login.Length > 16)
                error = "Podany login jest zbyt d³ugi!";
            else if (login.Length < 4)
                error = "Podany login jest zbyt krótki!";
            else if(password.Length > 16)
                error = "Podane has³o jest zbyt d³ugie!";
            else if (password.Length < 4)
                error = "Podane has³o jest zbyt krótkie!";
            else
                return true;

            onInvalidCallback?.Invoke(error);
            return false;
        }

        private void SubscribeEvents()
        {
            Debug.Log("Welcome state started...");
            manager.AssociatedUI.GetElement("btn_login").UI_element.ReturnAs<Button>().onClick.AddListener(TryLogin);
            manager.AssociatedUI.GetElement("btn_quit").UI_element.ReturnAs<Button>().onClick.AddListener(QuitGame);
        }
    }
}
