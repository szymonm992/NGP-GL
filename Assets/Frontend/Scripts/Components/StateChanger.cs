using Frontend.Scripts.Components;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Frontend.Scripts
{
    public class StateChanger : MonoBehaviour
    {
        [Inject] GameStateManager manager;
        // Start is called before the first frame update
        void Start()
        {
            manager.ChangeState(GameState.Calibration);
        }

        // Update is called once per frame
        void Update()
        {
        
        }
    }
}
