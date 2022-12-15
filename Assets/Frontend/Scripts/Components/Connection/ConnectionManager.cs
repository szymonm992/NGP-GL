using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Sfs2X.Entities;
using Sfs2X.Requests;
using Sfs2X.Entities.Data;

using Zenject;
using Sfs2X;
using GLShared.Networking;
using GLShared.Networking.Components;
using Frontend.Scripts.Models;
using Sfs2X.Core;
using Frontend.Scripts.Signals;
using Sfs2X.Util;

namespace Frontend.Scripts.Components
{
    public class ConnectionManager : MonoBehaviour
    {
        [Inject] private readonly SignalBus signalBus;
        [Inject] private readonly SmartFoxConnection smartFoxConnection;

        public void SendRoomJoinRequest(string cmd, ISFSObject data)
        {
            var room = smartFoxConnection.Connection.LastJoinedRoom;
            if (room == null)
            {
                Debug.LogError("Last joined room is null!");
                return;
            }
            ExtensionRequest request = new ExtensionRequest(cmd, data, room, false);
            smartFoxConnection.Connection.Send(request);
        }

        public void OnConnection(BaseEvent evt)
        {
            bool successfullyConnected = (bool)evt.Params["success"];

            signalBus.Fire(new ConnectionSignals.OnConnectionAttemptResult()
            {
                SuccessfullyConnected = successfullyConnected,
            });
        }

        public void OnLogin(BaseEvent evt)
        {
            signalBus.Fire(new ConnectionSignals.OnLoginAttemptResult()
            {
                SuccessfullyLogin = true,
                LoginMessage = string.Empty,
            });
        }

        public void OnLoginError(BaseEvent evt)
        {
            signalBus.Fire(new ConnectionSignals.OnLoginAttemptResult()
            {
                SuccessfullyLogin = false,
                LoginMessage = "Login failed: " + (string)evt.Params["errorMessage"],
            });
        }

        public void OnConnectionLost(BaseEvent evt)
        {
            string reason = (string)evt.Params["reason"];

            if (reason != ClientDisconnectionReason.MANUAL)
            {
                signalBus.Fire(new ConnectionSignals.OnDisconnectedFromServer()
                {
                    Reason = "Connection was lost; reason is: " + reason,
                }); 
            }
        }

        private void Update()
        {
            if (smartFoxConnection.IsInitialized)
            {
                smartFoxConnection.Connection.ProcessEvents();
            }
        }
    }
}
