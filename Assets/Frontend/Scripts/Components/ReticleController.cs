using Frontend.Scripts.Components.Temporary;
using Frontend.Scripts.Signals;
using GLShared.General.Interfaces;
using GLShared.General.ScriptableObjects;
using GLShared.General.Signals;
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
        [Inject] private readonly GameParameters gameParameters;

        [SerializeField] private RectTransform middleScreenCrosshair;
        [SerializeField] private RectTransform gunReticle;
        [SerializeField] private LayerMask gunMask;

        private bool isSniping = false;
        private Transform localPlayerGun;

        public void Initialize()
        {
            signalBus.Subscribe<BattleSignals.CameraSignals.OnCameraModeChanged>(OnCameraModeChanged);
            signalBus.Subscribe<PlayerSignals.OnPlayerInitialized>(OnPlayerInitialized);
        }

        private void OnPlayerInitialized(PlayerSignals.OnPlayerInitialized OnLocalPlayerInitialized)
        {
            var playerContext = OnLocalPlayerInitialized.PlayerProperties.PlayerContext;
            var vehicleController = playerContext.Container.Resolve<IVehicleController>();
            if (vehicleController.HasTurret)
            {
                var gunController = playerContext.Container.Resolve<ITurretController>();
                localPlayerGun = gunController.Gun;
            }
            else
            {
                gunReticle.gameObject.ToggleGameObjectIfActive(false);
            }
        }

        private void Update()
        {
            if (localPlayerGun != null)
            {
                Vector3 gunPosition = localPlayerGun.position + localPlayerGun.forward * gameParameters.GunMaxAimingDistance;

                if (Physics.Raycast(localPlayerGun.position, localPlayerGun.forward, out RaycastHit hit, gameParameters.GunMaxAimingDistance, gunMask))
                {
                    gunPosition = hit.point;
                }
                UpdateGunReticle(gunPosition);
            }
        }

        private void UpdateGunReticle(Vector3 gunPosition)
        {
            if (Vector3.Dot(Camera.main.transform.forward, gunPosition - Camera.main.transform.position) >= 0)
            {
                gunReticle.gameObject.ToggleGameObjectIfActive(true);

                Vector2 screenPosition = Camera.main.WorldToScreenPoint(gunPosition);
                gunReticle.position = Vector2.Lerp(gunReticle.position, screenPosition, Time.deltaTime * 20f);
            }
            else
            {
                gunReticle.gameObject.ToggleGameObjectIfActive(false);
            }
        }

        private void OnCameraModeChanged(BattleSignals.CameraSignals.OnCameraModeChanged OnCameraModeChanged)
        {
            isSniping = OnCameraModeChanged.IsSniping;

            float crosshairOffsetY = isSniping ? 0 : cameraController.ReticlePixelsOffset;
            middleScreenCrosshair.anchoredPosition = new Vector2(middleScreenCrosshair.anchoredPosition.x, crosshairOffsetY);
        }
    }
}
