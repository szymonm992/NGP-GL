using System;
using UnityEngine;
using Zenject;

namespace Frontend.Scripts.Components
{
    public abstract class GameStateEntity : IInitializable, ITickable, IDisposable
    {
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
            // optionally overridden
        }

        public virtual void QuitGame() => Application.Quit();

    }
}