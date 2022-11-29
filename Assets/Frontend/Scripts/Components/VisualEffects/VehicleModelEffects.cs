using Frontend.Scripts.Signals;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;
namespace Frontend.Scripts.Components
{
    public class VehicleModelEffects : MonoBehaviour, IInitializable
    {
        [Inject] private readonly Renderer[] allPlayerRenderers;
        [Inject] private readonly SignalBus signalBus;

        public void Initialize()
        {
            signalBus.Subscribe<BattleSignals.CameraSignals.OnCameraModeChanged>(OnCameraZoomChanged);
        }

        private void OnCameraZoomChanged(BattleSignals.CameraSignals.OnCameraModeChanged OnCameraZoomChanged)
        {
            if(allPlayerRenderers.Any())
            {
                foreach(var renderer in allPlayerRenderers)
                {
                    renderer.enabled = !OnCameraZoomChanged.IsSniping;
                }
            }
        }
    }
}
