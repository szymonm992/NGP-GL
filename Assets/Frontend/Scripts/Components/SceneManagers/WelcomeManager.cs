using Frontend.Scripts.Models;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Zenject;

namespace Frontend.Scripts.Components
{
    public class WelcomeManager : MonoBehaviour
    {
        [Inject] private readonly GameStateManager gameStateManager;
        [Inject] private readonly FormValidator formValidator;
        
        [SerializeField] private WelcomeUI associatedUI;
        public WelcomeUI AssociatedUI => associatedUI;

        private void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if(scene.name == "Welcome")
            {
                associatedUI.ClearError();
                gameStateManager.ChangeState(GameState.Welcome);
            }
        }
    }
}
