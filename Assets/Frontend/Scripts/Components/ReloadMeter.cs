using Frontend.Scripts.Signals;
using TMPro;
using UnityEngine;
using Zenject;

namespace Frontend.Scripts.Components
{
    public class ReloadMeter : MonoBehaviour, IInitializable
    {
        [Inject] private readonly SignalBus signalBus;
        [Inject] private readonly FrontendCameraController cameraController;
        [Inject(Id = "reloadTimer")] private readonly TextMeshProUGUI reloadTimerText;
        [Inject(Id = "reloadTransform")] private readonly RectTransform reloadTransform;
        
        public void DisplayCurrentReload(float currentReload)
        {
            reloadTimerText.text = $"{currentReload:F2}";
        }

        public void Initialize()
        {
            signalBus.Subscribe<BattleSignals.CameraSignals.OnCameraModeChanged>(OnCameraModeChanged);
        }

        private void OnCameraModeChanged(BattleSignals.CameraSignals.OnCameraModeChanged OnCameraModeChanged)
        {
            var isSniping = OnCameraModeChanged.Mode == Enums.CameraMode.Sniping;
            float crosshairOffsetY = isSniping ? 0 : cameraController.ReticlePixelsOffset;
            reloadTransform.anchoredPosition = new Vector2(reloadTransform.anchoredPosition.x, crosshairOffsetY);
        }
    }
}
