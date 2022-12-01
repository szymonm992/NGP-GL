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
        [Inject] private readonly TempGameManager tempGameManager;

        [SerializeField] private RectTransform middleScreenCrosshair;
        [SerializeField] private RectTransform gunReticle;
        [SerializeField] private LayerMask gunMask;


        private bool isSniping = false;
        private Transform gun;

        public void Initialize()
        {
            signalBus.Subscribe<BattleSignals.CameraSignals.OnCameraModeChanged>(OnCameraModeChanged);
            signalBus.Subscribe<PlayerSignals.OnLocalPlayerInitialized>(CreateLocalPlayerSettings);
        }

        private void CreateLocalPlayerSettings(PlayerSignals.OnLocalPlayerInitialized OnLocalPlayerInitialized)
        {
            var vehicleController = tempGameManager.PlayerContext.Container.Resolve<IVehicleController>();

            if (vehicleController.HasTurret)
            {
                var gunController = tempGameManager.PlayerContext.Container.Resolve<ITurretController>();
                gun = gunController.Gun;
            }
        }

        private void Update()
        {
            if (gun != null)
            {
                Vector3 gunPosition = gun.position + gun.forward * gameParameters.GunMaxAimingDistance;

                if (Physics.Raycast(gun.position, gun.forward, out RaycastHit hit, gameParameters.GunMaxAimingDistance, gunMask))
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
