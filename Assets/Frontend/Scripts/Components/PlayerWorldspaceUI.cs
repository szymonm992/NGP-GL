using GLShared.General.Interfaces;
using GLShared.General.Signals;
using GLShared.Networking.Components;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Zenject;

namespace Frontend.Scripts.Components
{
    public class PlayerWorldspaceUI : MonoBehaviour, IInitializable
    {
        [Inject] private readonly SignalBus signalBus;
        [Inject] private readonly PlayerEntity playerEntity;
        [Inject] private readonly IPlayerInstaller playerInstaller;
        [Inject(Id = "usernameLabel")] private readonly TextMeshProUGUI usernameLabel;

        public void Initialize()
        {
            signalBus.Subscribe<PlayerSignals.OnPlayerInitialized>(OnPlayerInitialized);
        }

        private void OnPlayerInitialized(PlayerSignals.OnPlayerInitialized OnPlayerInitialized)
        {
            bool isPrototyping = playerInstaller.IsPrototypeInstaller;
            string username = string.Empty;
            if (!isPrototyping)
            {
                if (OnPlayerInitialized.PlayerProperties.User.Name == playerEntity.Username)
                {
                    username = OnPlayerInitialized.PlayerProperties.User.Name;
                }
            }
            else
            {
                username = "localHost";
            }
            usernameLabel.text = username;
        }

        private void LateUpdate()
        {
            
        }
    }
}
