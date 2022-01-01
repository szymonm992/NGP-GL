using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
using UnityEngine.Scripting;
using System.Linq;
using UnityEngine.UI;


using Sfs2X;
using Sfs2X.Util;
using Sfs2X.Core;
using Sfs2X.Requests;
using Sfs2X.Entities.Data;

namespace Frontend.Scripts.Components
{
    public class GameplayState : GameStateEntity, IGameState
    {
        [Inject(Optional = true)] private GameplayManager manager;

        public override GameState ConnectedState { get; set; }

        public GameplayState(GameState st) => ConnectedState = st;

        public override void Start()
        {
            base.Start();
            Debug.Log("Gameplay started");
        }

        public override void Tick()
        {
            if (!IsActive) return;

        }
    }
}
