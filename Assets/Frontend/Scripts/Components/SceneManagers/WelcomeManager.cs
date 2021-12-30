using Frontend.Scripts.Models;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

namespace Frontend.Scripts.Components
{
    public class WelcomeManager : MonoBehaviour
    {
        [Inject] private readonly GameStateManager gameStateManager;
        
        [SerializeField] private WelcomeUI associatedUI;


        private void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if(scene.name == "Welcome")
            {
                associatedUI.ClearError();
                gameStateManager.ChangeState(GameState.Welcome);
            }
        }
    }
}
