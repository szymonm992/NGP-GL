using GLShared.General.Interfaces;
using GLShared.General.Models;
using GLShared.Networking.Components;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Frontend.Scripts.Components
{
    public class FrontTurretController : MonoBehaviour, ITurretController
    {
        [Inject] private readonly IPlayerInputProvider inputProvider;
        [Inject] private readonly PlayerEntity playerEntity;
        [Inject] private readonly IVehicleController vehicleController;
        [Inject] private readonly VehicleStatsBase vehicleStats;

        [SerializeField] private Transform gun;
        [SerializeField] private Transform turret;

        public Transform Turret => turret;
        public Transform Gun => gun;

        public bool TurretLock => inputProvider.TurretLockKey;

        private void LateUpdate()
        {
            if (!playerEntity.Properties.IsInitialized || TurretLock || vehicleController.IsUpsideDown)
            {
                return;
            }

            RotateTurret();
            RotateGun();
        }

        public void RotateGun()
        {
            if (gun != null)
            {
                gun.localRotation = Quaternion.RotateTowards(gun.localRotation,
               Quaternion.Euler(playerEntity.CurrentNetworkTransform.GunAngleX, gun.localEulerAngles.y, gun.localEulerAngles.z),
               Time.deltaTime * vehicleStats.GunRotationSpeed);
                //  playerEntity.CurrentNetworkTransform.TurretAngleY, turret.localEulerAngles.z), Time.deltaTime * vehicleStats.TurretRotationSpeed);
            }
        }

        public void RotateTurret()
        {
            if (turret != null)
            {
                turret.localRotation = Quaternion.RotateTowards(turret.localRotation, Quaternion.Euler(turret.localEulerAngles.x,
                    playerEntity.CurrentNetworkTransform.TurretAngleY, turret.localEulerAngles.z), 
                    Time.deltaTime * vehicleStats.TurretRotationSpeed);
            }
        }
    }
}
