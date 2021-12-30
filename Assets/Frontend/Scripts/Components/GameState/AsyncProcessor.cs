using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Frontend.Scripts.Components
{
    public class AsyncProcessor : MonoBehaviour
    {

        //Here we are calling all the monobehaviour things connected to state machine, e.g. coroutines
        public void StartNewCoroutine(Action callbackOnFinish, float delaySeconds)
        {
            StartCoroutine(WaitForAndLaunch(callbackOnFinish, delaySeconds));
        }

        private IEnumerator WaitForAndLaunch(Action callbackOnFinish, float delaySeconds)
        {
            yield return new WaitForSeconds(delaySeconds);
            callbackOnFinish?.Invoke();
        }

       
    }
}
