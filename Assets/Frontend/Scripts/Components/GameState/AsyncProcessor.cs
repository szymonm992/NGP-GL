using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;




namespace Frontend.Scripts.Components
{
    public class AsyncProcessor : MonoBehaviour
    {
        [Inject] private readonly GameStateManager gameStateManager;
        
        //Here we are calling all the monobehaviour things connected to state machine, e.g. coroutines
        public void StartNewCoroutine(Action callbackOnFinish, float delaySeconds)
        {
            StartCoroutine(WaitForAndLaunch(callbackOnFinish, delaySeconds));
        }

        private IEnumerator WaitForAndLaunch(Action callbackOnFinish, float delaySeconds)
        {
            yield return new WaitForSeconds(delaySeconds);
            gameStateManager.IsChangingState = false;
            callbackOnFinish?.Invoke();
            
        }


        private void Update()
        {
            
        }

    }
}
