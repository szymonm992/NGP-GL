using Frontend.Scripts.Components;
using Frontend.Scripts.Signals;
using GLShared.Networking.Components;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

namespace Frontend.Scripts
{
    public class SceneHandler : MonoBehaviour, IInitializable
    {
        [Inject] private readonly SignalBus signalBus;
        [Inject] private readonly SmartFoxConnection smartFox;
        [Inject] private readonly ConnectionManager connectionManager;


        public void Initialize()
        {
            signalBus.Subscribe<ConnectionSignals.OnDisconnectedFromServer>(OnDisconnect);
        }

        private void OnDisconnect(ConnectionSignals.OnDisconnectedFromServer OnDisconnectedFromServer)
        {
            Debug.Log("disc");
            var currentScene = SceneManager.GetActiveScene();
            if (currentScene.name != "Welcome")
            {
                smartFox.DisconnectError = "Connection lost with server!";
                SceneManager.LoadScene("Welcome");
            }
        }
    }
}
