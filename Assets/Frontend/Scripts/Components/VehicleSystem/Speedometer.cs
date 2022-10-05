using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Frontend.Scripts
{
    public class Speedometer : MonoBehaviour
    {
        [Inject] private readonly Rigidbody rig;

        [SerializeField] private Text velocityText;

        public void SetSpeedometr(float currentSpeed)
        {
            velocityText.text = $"{currentSpeed:F0}";
        }
    }
}
