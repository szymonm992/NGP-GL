using System;
using UnityEngine;
using Zenject;

namespace Frontend.Scripts.Components
{
    public abstract class GameStateEntity : IInitializable, ITickable, IDisposable, IGameState
    {
        [Inject] protected readonly GameStateManager gameStateManager;
        protected bool isActive = false;
        public bool IsActive => isActive;

        public virtual GameState ConnectedState { get; set; }

        public virtual void Initialize()
        {
            
            
            // optionally overridden
        }

        public virtual void Tick()
        {
            
            // optionally overridden
        }

        public virtual void Dispose()
        {
            isActive = false;
            // optionally overridden
        }

        public virtual void QuitGame() => Application.Quit();

        public virtual void Start()
        {
            isActive = true;
            Debug.Log("active");
        }
    }
}