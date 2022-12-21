using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Frontend.Scripts.Components
{
    public class FpsCounter : MonoBehaviour, IInitializable
    {
        [Inject(Id = "fpsMeter")] private readonly TextMeshProUGUI fpsMeter;

        private int lastFrameIndex;
        private float[] frameDeltaTimeArray;

        private void OnEnable()
        {
            frameDeltaTimeArray = new float[50];
        }

        private void Update()
        {
            frameDeltaTimeArray[lastFrameIndex] = Time.deltaTime;
            lastFrameIndex = (lastFrameIndex + 1) % frameDeltaTimeArray.Length;

            fpsMeter.text = "FPS: " + Mathf.RoundToInt(GetFPS()).ToString();
        }

        private float GetFPS()
        {
            float total = 0f;
            foreach(var deltaTime in frameDeltaTimeArray)
            {
                total += deltaTime;
            }
            return frameDeltaTimeArray.Length / total;
        }

        public void Initialize()
        {
        }
    }
}
