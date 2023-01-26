using Frontend.Scripts.Signals;
using GLShared.Networking.Components;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

namespace Frontend.Scripts.Components
{
    public class SceneHandler : MonoBehaviour, IInitializable
    {
        public const string WELCOME_SCENE_NAME = "Welcome";
        public const string MAIN_GAMEPLAY_SCENE_NAME = "MainTest";

        [Inject] private readonly SignalBus signalBus;
        [Inject] private readonly SmartFoxConnection smartFox;

        public void Initialize()
        {
            signalBus.Subscribe<ConnectionSignals.OnDisconnectedFromServer>(OnDisconnectedFromServer);
        }

        public AsyncOperation GetLoadSceneOperation(string name)
        {
            return SceneManager.LoadSceneAsync(name);
        }

        private void OnDisconnectedFromServer(ConnectionSignals.OnDisconnectedFromServer OnDisconnectedFromServer)
        {
            var currentScene = SceneManager.GetActiveScene();

            if (currentScene.name != WELCOME_SCENE_NAME)
            {
                smartFox.DisconnectError = OnDisconnectedFromServer.Reason;
                SceneManager.LoadScene(WELCOME_SCENE_NAME);
            }
        }
    }
}
