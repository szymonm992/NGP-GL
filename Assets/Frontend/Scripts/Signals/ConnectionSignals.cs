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
    }
}
