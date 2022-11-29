using Frontend.Scripts.Signals;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Frontend.Scripts.Components
{
    public class ReticleController : MonoBehaviour, IInitializable
    {
        [Inject] private readonly SignalBus signalBus;
        [Inject] private readonly FrontendCameraController cameraController;

        [SerializeField] private RectTransform middleScreenCrosshair;
        [SerializeField] private RectTransform gunReticle;

        private bool isSniping = false;

        public void Initialize()
        {
            signalBus.Subscribe<BattleSignals.CameraSignals.OnCameraModeChanged>(OnCameraModeChanged);
        }

        private void OnCameraModeChanged(BattleSignals.CameraSignals.OnCameraModeChanged OnCameraModeChanged)
        {
            isSniping = OnCameraModeChanged.IsSniping;

            float crosshairOffsetY = isSniping ? 0 : cameraController.ReticlePixelsOffset;
            middleScreenCrosshair.anchoredPosition = new Vector2(middleScreenCrosshair.anchoredPosition.x, crosshairOffsetY);
        }
    }
}
