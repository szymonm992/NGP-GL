using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Frontend.Scripts.Components
{
    public class AsyncProcessor : MonoBehaviour
    {

        //Leave empty, this one we use to call the coroutines and other monobehvaiour things 
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
