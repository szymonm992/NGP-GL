using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
using Frontend.Scripts.Enums;
namespace Frontend.Scripts.Interfaces
{
    public interface IGameState
    {
        public bool IsActive { get; set; }

        public GameState ConnectedState { get; set; }

        public void Start();
    }
}
