using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Frontend.Scripts.Components
{
    public interface IGameState
    {
        public bool IsActive { get; set; }

        public GameState ConnectedState { get; set; }

        public void Start();
    }
}
