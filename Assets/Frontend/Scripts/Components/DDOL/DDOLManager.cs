using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Frontend.Scripts.Components
{
    public class DDOLManager : MonoBehaviour
    {
        private void Awake() => DontDestroyOnLoad(this.gameObject);

        public void QuitGame()
        {
            Application.Quit();
        }
    }
}
