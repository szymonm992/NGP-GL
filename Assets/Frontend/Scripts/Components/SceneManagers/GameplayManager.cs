using Frontend.Scripts.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
namespace Frontend.Scripts.Components
{
    public class GameplayManager : MonoBehaviour
    {
        [Inject(Optional = true)] private readonly GameStateManager gameStateManager;


        private void Start()
        {
            gameStateManager.ChangeState(GameState.Gameplay);
        }

    }
}
