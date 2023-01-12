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
    public class PlayerUI : MonoBehaviour, IInitializable
    {
        [Inject] private readonly SignalBus signalBus;
        [Inject] private readonly PlayerEntity playerEntity;
        [Inject] private readonly IPlayerInstaller playerInstaller;
        [Inject] private readonly RectTransform rectTransform;
        [Inject(Id = "usernameLabel")] private readonly TextMeshProUGUI usernameLabel;
        [Inject(Id = "playerUIHolder")] public Transform uiHolder;

        public void Initialize()
        {
            signalBus.Subscribe<PlayerSignals.OnPlayerInitialized>(OnPlayerInitialized);
        }

        private void OnPlayerInitialized(PlayerSignals.OnPlayerInitialized OnPlayerInitialized)
        {
            bool isPrototyping = playerInstaller.IsPrototypeInstaller;

            if (!isPrototyping)
            {
                if (OnPlayerInitialized.PlayerProperties.User.Name == playerEntity.Username)
                {
                    usernameLabel.text = playerEntity.Username;
                }
            }
            else
            {
                usernameLabel.text = "localHost";
            }
        }

        private void LateUpdate()
        {
            if (uiHolder != null)
            {
                if (Vector3.Dot(Camera.main.transform.forward, uiHolder.position - Camera.main.transform.position) >= 0)
                {
                    rectTransform.gameObject.ToggleGameObjectIfActive(true);
                    rectTransform.position = Camera.main.WorldToScreenPoint(uiHolder.position);
                }
                else
                {
                    rectTransform.gameObject.ToggleGameObjectIfActive(false);
                }
            }
        }
    }
}
