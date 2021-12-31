using System;
using UnityEngine;
using Zenject;

namespace Frontend.Scripts.Components
{
    public abstract class GameStateEntity : IInitializable, ITickable, IDisposable, IGameState
    {
        [Inject] protected GameStateManager gameStateManager;
  
        public bool IsActive { get; set; }

        public virtual GameState ConnectedState { get; set; }

        public virtual void Initialize()
        {
            
            
            // optionally overridden
        }

        public virtual void Tick()
        {
            
            // optionally overridden
        }

        public virtual void Dispose() => this.IsActive = false;


        public virtual void QuitGame() => Application.Quit();

        public virtual void Start() => this.IsActive = true;

    }
}