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

        private bool isInsideCameraView;

        public bool IsInsideCameraView => isInsideCameraView;

        public void Initialize()
        {
            signalBus.Subscribe<BattleSignals.CameraSignals.OnCameraModeChanged>(OnCameraZoomChanged);
        }

        private void OnCameraZoomChanged(BattleSignals.CameraSignals.OnCameraModeChanged OnCameraZoomChanged)
        {
            if (OnCameraZoomChanged.playerObject != gameObject)
            {
                return;
            }

            if(allPlayerRenderers.Any())
            {
                foreach(var renderer in allPlayerRenderers)
                {
                    renderer.enabled = !OnCameraZoomChanged.IsSniping;
                }
            }
        }

        private void Update()
        {
            isInsideCameraView = (Vector3.Dot(Camera.main.transform.forward, transform.position - Camera.main.transform.position) >= 0);
        }
    }
}
