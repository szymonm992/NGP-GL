using Automachine.Scripts.Components;
using Frontend.Scripts.Enums;
using GLShared.General.Enums;
using GLShared.Networking.Components;
using Sfs2X;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Frontend.Scripts.Components.GameState
{
    public class WelcomeOnLobbyJoining : State<WelcomeStage>
    {
        [Inject] private SmartFoxConnection smartFox;

        public override void StartState()
        {
            base.StartState();
            Debug.Log("Joining lobby "+smartFox.IsInitialized);
        }
    }
}
