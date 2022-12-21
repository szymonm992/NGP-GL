using System.Collections;
using System.Collections.Generic;
using Zenject;
using UnityEngine;
using Frontend.Scripts.Signals;
using System.Net.NetworkInformation;
using TMPro;

namespace Frontend.Scripts.Components
{
    public class PingCounter : MonoBehaviour, IInitializable
    {
        [Inject] private readonly SignalBus signalBus;
        [Inject(Id = "pingMeter")] private readonly TextMeshProUGUI pingMeter;

        public void Initialize()
        {
            signalBus.Subscribe<ConnectionSignals.OnPingUpdate>(OnPingUpdate);
        }

        private void OnPingUpdate(ConnectionSignals.OnPingUpdate OnPingUpdate)
        {
            pingMeter.text = "Ping: " + OnPingUpdate.CurrentAveragePing.ToString("F0") + "ms";
        }
    }
}
