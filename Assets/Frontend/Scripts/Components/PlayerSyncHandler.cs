using GLShared.General.Signals;
using GLShared.Networking.Components;
using GLShared.Networking.Interfaces;
using GLShared.Networking.Models;
using UnityEngine;
using Zenject;

namespace Frontend.Scripts.Components
{
    public class PlayerSyncHandler : MonoBehaviour, IInitializable, ISyncInterpolator
    {
        private const float TANK_DEVIATION_CORRECTION_FACTOR = 5.0f;

        [Inject] private readonly SignalBus signalBus;
        [Inject] private readonly TimeManager timeManager;
        [Inject] private readonly Speedometer speedometer;
        [Inject] private readonly PlayerEntity playerEntity;
        [Inject(Optional = true)] private readonly FrontTurretController turretController;

        private NetworkTransform currentNetworkTransform;
        private NetworkTransform previousNetworkTransform;

        private NetworkTransform currentLocalTransform;

        private int receivedTransforms;
        private float timeSinceCurrentTransform;

        public bool IsRunning { get; private set; } = false;

        public void Initialize()
        {
            signalBus.Subscribe<PlayerSignals.OnPlayerInitialized>(OnPlayerInitialized);
            receivedTransforms = 0;
        }

        public void ProcessCurrentNetworkTransform(INetworkTransform nTransform)
        {
            if (!IsRunning)
            {
                return;
            }

            var networkTransform = (NetworkTransform)nTransform;

            if (receivedTransforms == 0)
            {
                currentLocalTransform = networkTransform;
            }

            if (receivedTransforms < 1 || networkTransform.TimeStamp >= currentNetworkTransform.TimeStamp)
            {
                previousNetworkTransform = currentNetworkTransform;
                currentNetworkTransform = networkTransform;
                timeSinceCurrentTransform = 0.0f;
            }
            else if (receivedTransforms < 2 || networkTransform.TimeStamp >= previousNetworkTransform.TimeStamp)
            {
                previousNetworkTransform = networkTransform;
            }
            else
            {
                return;
            }

            receivedTransforms++;
        }

        private void Update()
        {
            if (!IsRunning || receivedTransforms == 0)
            {
                return;
            }

            Interpolate();

            timeSinceCurrentTransform += Time.deltaTime;
        }

        private void Interpolate()
        {
            // Position and rotation are extrapolated with small error correction.
            // Everything else is interpolated with big error correction.

            if (!IsRunning || receivedTransforms == 0)
            {
                return;
            }

            if (receivedTransforms < 2 || currentNetworkTransform.TimeStamp == previousNetworkTransform.TimeStamp)
            {
                ApplyTransform(currentNetworkTransform);
                return;
            }

            var currentMoveStep = CalculateMoveStep(
                currentNetworkTransform,
                timeSinceCurrentTransform
            );

            var localMoveStep = CalculateMoveStep(
                currentLocalTransform,
                Time.deltaTime
            );

            // TODO: Extrapolated turret makes reticle overshoot its target. However, interpolating it causes
            // a desync of the reticle when tank is in motion. This could probably be prevented if server
            // sent some additional information to clients (for instance, target turret and gun angles).

            var deviationFactor = Mathf.Min(1.0f, TANK_DEVIATION_CORRECTION_FACTOR * Time.deltaTime);
            var lerpedTransform = LerpNetworkTransforms(localMoveStep, currentMoveStep, deviationFactor);
            ApplyTransform(lerpedTransform);
        }

        private NetworkTransform CalculateMoveStep(NetworkTransform origin, float stepTime)
        {
            var deltaA = previousNetworkTransform;
            var deltaB = currentNetworkTransform;

            var deltaPosition = deltaB.Position - deltaA.Position;
            var deltaRotation = deltaB.Rotation * Quaternion.Inverse(deltaA.Rotation);
            var deltaGunAngle = Mathf.DeltaAngle(deltaA.GunAngleX, deltaB.GunAngleX);
            var deltaTurretAngle = Mathf.DeltaAngle(deltaA.TurretAngleY, deltaB.TurretAngleY);
            var deltaSpeed = deltaB.CurrentSpeed - deltaA.CurrentSpeed;
            var deltaTurningSpeed = deltaB.CurrentTurningSpeed - deltaA.CurrentTurningSpeed;

            float stepMultiplier = (float)(stepTime / ((deltaB.TimeStamp - deltaA.TimeStamp) / 1000.0f));

            return new NetworkTransform
            {
                Identifier = origin.Identifier,
                Position = origin.Position + deltaPosition * stepMultiplier,
                EulerAngles = Quaternion.Slerp(origin.Rotation, deltaRotation * origin.Rotation, stepMultiplier).eulerAngles,
                GunAngleX = origin.GunAngleX + deltaGunAngle * stepMultiplier,
                TurretAngleY = origin.TurretAngleY + deltaTurretAngle * stepMultiplier,
                CurrentSpeed = origin.CurrentSpeed + deltaSpeed * stepMultiplier,
                CurrentTurningSpeed = origin.CurrentTurningSpeed + deltaTurningSpeed * stepMultiplier,
                TimeStamp = origin.TimeStamp + stepTime * 1000.0f
            };
        }

        private NetworkTransform LerpNetworkTransforms(NetworkTransform a, NetworkTransform b, float factor)
        {
            return new NetworkTransform
            {
                Identifier = b.Identifier,
                Position = Vector3.Lerp(a.Position, b.Position, factor),
                EulerAngles = Quaternion.Slerp(a.Rotation, b.Rotation, factor).eulerAngles,
                GunAngleX = Mathf.LerpAngle(a.GunAngleX, b.GunAngleX, factor),
                TurretAngleY = Mathf.LerpAngle(a.TurretAngleY, b.TurretAngleY, factor),
                CurrentSpeed = Mathf.Lerp(a.CurrentSpeed, b.CurrentSpeed, factor),
                CurrentTurningSpeed = Mathf.Lerp(a.CurrentTurningSpeed, b.CurrentTurningSpeed, factor),
                TimeStamp = a.TimeStamp * (1.0f - factor) + b.TimeStamp * factor
            };
        }

        private void ApplyTransform(NetworkTransform appliedTransform)
        {
            currentLocalTransform = appliedTransform;

            transform.SetPositionAndRotation(appliedTransform.Position, appliedTransform.Rotation);
            turretController.SetTurretAndGunRotation(appliedTransform.TurretAngleY, appliedTransform.GunAngleX);

            if (playerEntity.IsLocalPlayer)
            {
                speedometer.SetSpeedometr(appliedTransform.CurrentSpeed);
            }
        }

        private void OnPlayerInitialized(PlayerSignals.OnPlayerInitialized signal)
        {
            IsRunning = true;
        }
    }
}
