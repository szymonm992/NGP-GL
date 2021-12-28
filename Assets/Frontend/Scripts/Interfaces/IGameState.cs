using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Frontend.Scripts.Components
{
    public interface IGameState
    {
        public GameState ConnectedState { get; set; }
        void Startt();
    }
}
