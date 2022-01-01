using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Frontend.Scripts
{
    public interface IPlayerInput 
    {
        public float Vertical { get; }
        public float Horizontal { get; }
        public bool Brake { get; }

    }
}
