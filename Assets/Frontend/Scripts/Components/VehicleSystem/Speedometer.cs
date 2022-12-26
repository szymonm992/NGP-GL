using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Frontend.Scripts.Components
{
    public class Speedometer : MonoBehaviour
    {
        [Inject(Id ="speedometer")] private TextMeshProUGUI velocityText;

        public void SetSpeedometr(float currentSpeed)
        {
            if(velocityText != null)
            {
                velocityText.text = $"{currentSpeed:F0}";
            }
        }
    }
}
