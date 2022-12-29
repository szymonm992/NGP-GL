using GLShared.General.Interfaces;
using GLShared.General.Models;
using GLShared.General.ScriptableObjects;
using GLShared.General.Signals;
using GLShared.Networking.Components;
using Sfs2X.Entities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Prototyping.Scripts.Components
{
    public class PrototypingLevelManager : MonoBehaviour, IInitializable
    {
        [Inject] private readonly SignalBus signalBus;
        [Inject] protected readonly IVehiclesDatabase vehicleDatabase;
        [Inject] protected readonly PlayerSpawner playerSpawner;

        public void Initialize()
        {
            StartCoroutine(CreatePlayerRoutine(0.5f));
        }

        private void CreatePlayer(string vehicleName, Vector3 spawnPosition, Vector3 spawnEulerAngles)
        {
            var playerProperties = GetPlayerInitData(vehicleName, spawnPosition, spawnEulerAngles);

            var prefabEntity = playerProperties.PlayerContext.gameObject.GetComponent<PlayerEntity>();//this references only to prefab
            var playerEntity = playerSpawner.Spawn(prefabEntity, playerProperties);
        }


        protected PlayerProperties GetPlayerInitData(string vehicleName, Vector3 spawnPosition, Vector3 spawnEulerAngles)
        {
            var vehicleData = vehicleDatabase.GetVehicleInfo(vehicleName);
            if (vehicleData != null)
            {
                return new PlayerProperties()
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
            CreatePlayer("T-55", new Vector3(132.35f, 2f, 118.99f), new Vector3(0, 90f, 0));
            signalBus.Fire(new PlayerSignals.OnAllPlayersInputLockUpdate()
            {
                LockPlayersInput = false,
            });
        }

    }
}
