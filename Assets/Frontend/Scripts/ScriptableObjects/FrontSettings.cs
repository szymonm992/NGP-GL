using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Frontend.Scripts.ScriptableObjects
{
    [CreateAssetMenu(fileName = "FrontSettings", menuName = "UT/Frontend/Frontend settings")]
    public class FrontSettings : ScriptableObject
    {
        [Header("Network settings")]
        [SerializeField] private bool connectLocalhost = false;
        [SerializeField] private string localhostAddress = "127.0.0.1";
        [SerializeField] private string wanAddress = "185.157.82.150";

        [Header("Prototyping settings")]
        [SerializeField] private int prototypePlayersAmount = 1;

        public bool ConnectLocalhost => connectLocalhost;
        public string LocalhostAddress => localhostAddress;
        public string WanAddress => wanAddress;
        public int PrototypePlayersAmount => prototypePlayersAmount;

    }
}
