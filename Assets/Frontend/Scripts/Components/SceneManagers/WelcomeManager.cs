using Frontend.Scripts.Models;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Frontend.Scripts.Components
{
    public class WelcomeManager : MonoBehaviour
    {
        [Inject] private readonly GameStateManager gameStateManager;
        
        [SerializeField] private WelcomeUI associatedUI;

       
        private void Start()
        {
            gameStateManager.ChangeState(GameState.Welcome);
        }


        private void Update()
        {
            

        }
    }
}
