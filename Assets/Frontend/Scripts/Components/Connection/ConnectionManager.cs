using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Sfs2X.Entities;
using Sfs2X.Requests;
using Sfs2X.Entities.Data;

using Zenject;
using Sfs2X;

namespace Frontend.Scripts.Components
{
    public class ConnectionManager : MonoBehaviour
    {

        [Inject] private readonly SmartFoxConnection smartFoxConnection;
        public const string HOST = "185.157.80.18";
        public const int PORT = 9933;

        public void SendRoomJoinRequest(string cmd, ISFSObject data)
        {
            var room = smartFoxConnection.Connection.LastJoinedRoom;
            if(room == null)
            {
                Debug.LogError("Last joined room is null!");
                return;
            }
            ExtensionRequest request = new ExtensionRequest(cmd, data, room, false);
            smartFoxConnection.Connection.Send(request);
        }

       

    }
}
