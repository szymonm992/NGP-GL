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
    public class WelcomeState : GameStateEntity, IGameState
    {
       
        [Inject] private WelcomeManager manager;
        [Inject] private readonly FormValidator formValidator;
   
        public override GameState ConnectedState { get; set; }

        private string loadedLogin;
        private string loadedPassword;

        private InputField loginField;
        private InputField passwordField;

        public WelcomeState(GameState st) => ConnectedState = st;

        public override void Start()
        {
            base.Start();
            SubscribeEvents();
        }

        public override void Tick()
        {
            if (!IsActive) return;

            if(Input.GetKey(KeyCode.Tab))
            {
                FocusOnPasswordField();
            }
        }

        public void TryLogin()
        {
            loadedLogin = loginField.text;
            loadedPassword = passwordField.text;
            if (formValidator.IsLoginFormValid(loadedLogin, loadedPassword, manager.AssociatedUI.DisplayError))
            {
                Debug.Log("Valid credentials client-side");
                manager.AssociatedUI.ToggleUI(false);
                gameStateManager.ChangeState(GameState.OnLogin);
            }
            
        }

        private void SubscribeEvents()
        {
            Debug.Log("Welcome state started...");
            manager.AssociatedUI.GetElement("btn_login").ReturnAs<Button>().onClick.AddListener(TryLogin);
            manager.AssociatedUI.GetElement("btn_quit").ReturnAs<Button>().onClick.AddListener(QuitGame);

            loginField = manager.AssociatedUI.GetElement("inp_login").ReturnAs<InputField>();
            passwordField = manager.AssociatedUI.GetElement("inp_pwd").ReturnAs<InputField>();
        }

        private void FocusOnPasswordField()
        {
            if (loginField.isFocused)
            {
                passwordField.Select();
            }
        }

    }
}
