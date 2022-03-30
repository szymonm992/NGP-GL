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
using Frontend.Scripts.Enums;
using Frontend.Scripts.Interfaces;

namespace Frontend.Scripts.Components
{
    public class GameplayState : GameStateEntity, IGameState
    {
        [Inject(Optional = true)] private GameplayManager manager;
        [Inject] private readonly CameraController controller;
        public override GameState ConnectedState { get; set; }

        public GameplayState(GameState st) => ConnectedState = st;

        public override void Start()
        {
            base.Start();
            controller.AssignController(manager.playerContext, manager.playerContext.gameObject.transform.eulerAngles);
            Debug.Log("Gameplay started");
        }

        public override void Tick()
        {
            if (!IsActive) return;

        }
    }
}
