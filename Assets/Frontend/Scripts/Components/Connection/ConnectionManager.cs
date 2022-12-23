using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Sfs2X.Entities;
using Sfs2X.Requests;
using Sfs2X.Entities.Data;

using Zenject;
using GLShared.Networking;
using GLShared.Networking.Components;
using Frontend.Scripts.Models;
using Sfs2X.Core;
using Frontend.Scripts.Signals;
using Sfs2X.Util;
using System;

namespace Frontend.Scripts.Components
{
    public class ConnectionManager : MonoBehaviour
    {
        [Inject] private readonly SignalBus signalBus;
        [Inject] private readonly SmartFoxConnection smartFoxConnection;

        public void SendRoomJoinRequest(string cmd, ISFSObject data)
        {
            var room = smartFoxConnection.Connection.LastJoinedRoom;
            ExtensionRequest request = new ExtensionRequest(cmd, data, room, false);
            smartFoxConnection.Connection.Send(request);
        }

        public void SendRequest(string cmd, ISFSObject data = null)
        {
            if (data == null)
            {
                data = new SFSObject();
            }
            ExtensionRequest request = new ExtensionRequest(cmd, data);
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

        public void OnLogout(BaseEvent evt)
        {
            signalBus.Fire(new ConnectionSignals.OnLoginAttemptResult()
            {
                SuccessfullyLogin = false,
                LoginMessage = string.Empty,
            });
        }

        public void OnLoginError(BaseEvent _)
        {
            signalBus.Fire(new ConnectionSignals.OnDisconnectedFromServer()
            {
                Reason = string.Empty,
            });
        }

        public void OnConnectionLost(BaseEvent evt)
        {
            string reason = (string)evt.Params["reason"];

            if (reason != ClientDisconnectionReason.MANUAL)
            {
                signalBus.Fire(new ConnectionSignals.OnDisconnectedFromServer()
                {
                    Reason = "Disconnected from server. Server shutdown",
                });
            }
            else
            {
                signalBus.Fire(new ConnectionSignals.OnDisconnectedFromServer()
                {
                    Reason = "Disconnected from server. "+smartFoxConnection.DisconnectError,
                });
                smartFoxConnection.DisconnectError = string.Empty;
            }
        }

        public void OnRoomJoin(BaseEvent _)
        {
            var room = smartFoxConnection.Connection.LastJoinedRoom;
            if (room.Name == "Lobby")
            {
                signalBus.Fire(new ConnectionSignals.OnLobbyJoinAttemptResult()
                {
                    SuccessfullyJoinedLobby = true,
                    LobbyJoinMessage = string.Empty,
                });
            }
            else
            {
                signalBus.Fire(new ConnectionSignals.OnRoomJoinResponse()
                {
                    SuccessfullyJoinedRoom = true,
                    RoomJoinMessage = string.Empty,
                });
            }
        }

        public void OnRoomJoinError(BaseEvent evt)
        {
            var room = smartFoxConnection.Connection.LastJoinedRoom;
            var reason = "Room join failed: " + (string)evt.Params["errorMessage"];
            if (room == null)
            {
                signalBus.Fire(new ConnectionSignals.OnLobbyJoinAttemptResult()
                {
                    SuccessfullyJoinedLobby = false,
                    LobbyJoinMessage = reason,
                });
            }
            else
            {
                signalBus.Fire(new ConnectionSignals.OnRoomJoinResponse()
                {
                    SuccessfullyJoinedRoom = false,
                    RoomJoinMessage = reason,
                });
            }
        }

        public void OnExtensionResponse(BaseEvent evt)
        {
            try
            {
                string cmd = (string)evt.Params["cmd"];
                ISFSObject data = (SFSObject)evt.Params["params"];
                if (cmd == "userInitialVariables")
                {
                    //HandleInitialVariables(objIn);
                }
                if (cmd == "lobbyJoinResult")
                {
                    HandleLobbyJoinResult(data);
                }
                if(cmd == "getServerSettings")
                {
                    signalBus.Fire(new ConnectionSignals.OnServerSettingsResponse()
                    { 
                        ServerSettingsData = data,
                    });
                }

            }
            catch (System.Exception exception)
            {
                Debug.Log("Exception handling response: " + exception.Message
                   + " >>>[AND TRACE IS]>>> " + exception.StackTrace);
            }
        }

        private void HandleLobbyJoinResult(ISFSObject objIn)
        {
            if (objIn.ContainsKey("result"))
            {
                string result = objIn.GetUtfString("result");
                if (result == "success")
                {
                }
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
