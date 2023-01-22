using Frontend.Scripts.Components;
using Frontend.Scripts.ScriptableObjects;
using GLShared.General.Interfaces;
using GLShared.General.Models;
using GLShared.General.Signals;
using GLShared.Networking.Components;
using System.Collections;
using TMPro;
using UnityEngine;
using Zenject;

namespace Prototyping.Scripts.Components
{
    public class PrototypingLevelManager : MonoBehaviour, IInitializable
    {
        [Inject] private readonly SignalBus signalBus;
        [Inject] protected readonly IVehiclesDatabase vehicleDatabase;
        [Inject] protected readonly PlayerSpawner playerSpawner;
        [Inject] protected readonly FrontSettings frontSettings;
        [Inject(Optional = true)] protected readonly Speedometer speedometer;
        [Inject(Id = "countdownText")] private readonly TextMeshProUGUI countdownText;
        [Inject(Id = "pingMeter")] private readonly TextMeshProUGUI pingText;

        private PlayerEntity mockCurrentPlayer = null;

        public void Initialize()
        {
            StartCoroutine(CreatePlayerRoutine(0.5f));
            countdownText.text = string.Empty;
            pingText.text = string.Empty;
        }

        private void CreatePlayer(string vehicleName, Vector3 spawnPosition, Vector3 spawnEulerAngles)
        {
            var playerProperties = GetPlayerInitData(vehicleName, spawnPosition, spawnEulerAngles);

            var prefabEntity = playerProperties.PlayerContext.gameObject.GetComponent<PlayerEntity>();//this references only to prefab
            var playerEntity = playerSpawner.Spawn(prefabEntity, playerProperties);
            mockCurrentPlayer = playerEntity;
        }


        protected PlayerProperties GetPlayerInitData(string vehicleName, Vector3 spawnPosition, Vector3 spawnEulerAngles)
        {
            var vehicleData = vehicleDatabase.GetVehicleInfo(vehicleName);
            if (vehicleData != null)
            {
                return new()
                {
                    PlayerContext = vehicleData.VehiclePrefab,
                    PlayerVehicleName = vehicleData.VehicleName,
                    IsLocal = true,
                    SpawnPosition = spawnPosition,
                    SpawnRotation = Quaternion.Euler(spawnEulerAngles.x, spawnEulerAngles.y, spawnEulerAngles.z),
                    User = null,
                };
            }
            return null;
        }

        private IEnumerator CreatePlayerRoutine(float delay)
        {
            yield return new WaitForSeconds(delay);
            for (int i = 0; i < frontSettings.PrototypePlayersAmount; i++)
            {
                CreatePlayer("T-55", new(677, 24, 620f + (8 * i)), new(0, 90f, 0));
            }
           
            signalBus.Fire(new PlayerSignals.OnAllPlayersInputLockUpdate()
            {
                LockPlayersInput = false,
            });
        }

        private void Update()
        {
            if (mockCurrentPlayer != null && speedometer != null)
            {
                speedometer.SetSpeedometr(mockCurrentPlayer.EntityVelocity);
            }
        }
    }
}
