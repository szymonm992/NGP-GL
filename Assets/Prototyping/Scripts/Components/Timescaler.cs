using UnityEngine;

namespace Frontend.Prototyping.Scripts
{
    public class Timescaler : MonoBehaviour
    {
        [SerializeField] private KeyCode SlowerTimescaleKey;
        [SerializeField] private KeyCode HigherTimescaleKey;

        void Update()
        {
            var prevTimescale = Time.timeScale;

            if (Input.GetKeyDown(SlowerTimescaleKey))
            {
                Time.timeScale *= 0.5f;

            }
            if (Input.GetKeyDown(HigherTimescaleKey) && Time.timeScale < 8.0f)
            {
                Time.timeScale *= 2.0f;
            }

            if(prevTimescale != Time.timeScale)
            {
                Debug.Log($"Current timescale: {Time.timeScale}");
            }
        }
    }
}
