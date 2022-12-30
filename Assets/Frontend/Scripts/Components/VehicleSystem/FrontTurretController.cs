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

        [SerializeField] private Transform gun;
        [SerializeField] private Transform turret;

        public Transform Turret => turret;
        public Transform Gun => gun;

        public bool TurretLock => inputProvider.TurretLockKey;

        public void SetTurretAndGunRotation(float turretY, float gunX)
        {
            gun.localEulerAngles = new Vector3(gunX, gun.localEulerAngles.y, gun.localEulerAngles.z);
            turret.localEulerAngles = new Vector3(turret.localEulerAngles.x, turretY, turret.localEulerAngles.z);
        }

        public void RotateGun()
        {
        }

        public void RotateTurret()
        {
        }
    }
}
