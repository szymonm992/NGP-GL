using Sfs2X.Entities.Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Frontend.Scripts.Signals
{
    public class ConnectionSignals
    {
        public class OnConnectionAttemptResult
        {
            public bool SuccessfullyConnected { get; set; }
        }

        public class OnLoginAttemptResult
        {
            public bool SuccessfullyLogin { get; set; }
            public string LoginMessage { get; set; }
        }

        public class OnDisconnectedFromServer
        {
            public string Reason { get; set; }
        }

        public class OnLobbyJoinAttemptResult
        {
            public bool SuccessfullyJoinedLobby { get; set; }
            public string LobbyJoinMessage { get; set; }
        }

        public class OnServerSettingsResponse
        {
            public ISFSObject ServerSettingsData { get; set; }
        }

        public class OnBattleJoiningInfoReceived
        {
            public ISFSObject BattleJoiningInfoData { get; set; }
        }

        public class OnRoomJoinResponse
        {
            public bool SuccessfullyJoinedRoom { get; set; }
            public string RoomJoinMessage { get; set; }
        }

        public class OnCancelEnteringBattle
        {
            public bool SuccessfullyCanceled { get; set; }
        }

        public class OnPingUpdate
        {
            public double CurrentAveragePing { get; set; }
        }

        
    }
}
