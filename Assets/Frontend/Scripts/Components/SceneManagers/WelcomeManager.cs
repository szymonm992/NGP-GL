using Frontend.Scripts.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Frontend.Scripts.Components
{
    public class WelcomeManager : MonoBehaviour
    {
        [Inject] private readonly GameStateManager gameStateManager;
        [SerializeField] private WelcomeUI associatedUI;
        public WelcomeUI AssociatedUI { get { return associatedUI; } }
    
        private void Start()
        {
           associatedUI.ClearError();
           gameStateManager.ChangeState(GameState.Welcome);

        }
     
    }
}
