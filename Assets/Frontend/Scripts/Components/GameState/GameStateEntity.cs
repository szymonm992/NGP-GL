using System;
using UnityEngine;
using Zenject;

namespace Frontend.Scripts.Components
{
    public abstract class GameStateEntity : IInitializable, ITickable, IDisposable, IGameState
    {
        [Inject] protected readonly GameStateManager gameStateManager;

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

        /// <summary>
        /// Whenever we override we should remember about base (activation of state)
        /// </summary>
        public virtual void Dispose() => this.IsActive = false;
        /// <summary>
        /// Whenever we override we should remember about base (activation of state)
        /// </summary>
        public virtual void Start() => this.IsActive = true;

        public virtual void QuitGame() => Application.Quit();
    }
}