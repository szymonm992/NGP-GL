using GLShared.General.Interfaces;
using GLShared.Networking.Components;
using GLShared.Networking.Interfaces;
using GLShared.Networking.Models;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Frontend.Scripts
{
    public class FrontShellController : MonoBehaviour, IShellController, ISyncInterpolator
    {
        [Inject] private readonly ShellEntity shellEntity;

        public string OwnerUsername => shellEntity.Properties.Username;

        public void Initialize()
        {
            Debug.Log("shell initialized");
        }

        public void ProcessCurrentNetworkTransform(NetworkTransform nTransform)
        {
            Debug.Log("processing shell transform");
        }
    }
}

