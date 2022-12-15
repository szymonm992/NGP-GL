using Automachine.Scripts.Components;
using Frontend.Scripts.Enums;
using GLShared.General.Enums;
using GLShared.Networking.Components;
using Sfs2X;
using Sfs2X.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.MemoryProfiler;
using UnityEngine;
using Zenject;

namespace Frontend.Scripts.Components.GameState
{
    public class WelcomeOnLobbyJoined : State<WelcomeStage>
    {
        [Inject] private readonly SmartFoxConnection smartFox;
        [Inject(Id = "welcomeCanvas")] private readonly RectTransform welcomeUi;
        [Inject(Id = "lobbyCanvas")] private readonly RectTransform lobbyCanvas;
        public override void StartState()
        {
            base.StartState();
            welcomeUi.gameObject.ToggleGameObjectIfActive(false);
            lobbyCanvas.gameObject.ToggleGameObjectIfActive(true);
        }
    }
}
