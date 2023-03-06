using Frontend.Scripts.Test;
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
        private const float TIME_DEVIATION_ALLOWANCE_SCALE = 0.99f;

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

        //private LagMachine lagMachine;

        public bool IsRunning { get; private set; } = false;

        public void Initialize()
        {
            signalBus.Subscribe<PlayerSignals.OnPlayerInitialized>(OnPlayerInitialized);
            receivedTransforms = 0;

            //lagMachine = new LagMachine
            //{
            //    MinLength = 0.1f,
            //    MaxLength = 1.0f,
            //    MinSpike = 0.3f,
            //    MaxSpike = 0.5f,
            //    PacketMissChance = 0.5f
            //};
        }

        public void ProcessCurrentNetworkTransform(INetworkTransform nTransform)
        {
            if (!IsRunning)
            {
                return;
            }

            var networkTransform = (NetworkTransform)nTransform;

            //lagMachine.InvokeLagged(delegate{ 
            ApplyNetworkTransfor(networkTransform); 
            //});
        }

        private void ApplyNetworkTransfor(NetworkTransform networkTransform)
        {
            if (!IsRunning)
            {
                return;
            }

            if (receivedTransforms == 0)
            {
                currentLocalTransform = networkTransform;
                timeSinceCurrentTransform = 0.0f;
            }

            if (receivedTransforms < 1 || networkTransform.TimeStamp >= currentNetworkTransform.TimeStamp)
            {
                if(receivedTransforms >= 2)
                {
                    timeSinceCurrentTransform -= (currentNetworkTransform.TimeStamp - previousNetworkTransform.TimeStamp) / 1000.0f;
                    timeSinceCurrentTransform *= TIME_DEVIATION_ALLOWANCE_SCALE;
                }
                else if(receivedTransforms == 1)
                {
                    timeSinceCurrentTransform = 0.0f;
                }

                previousNetworkTransform = currentNetworkTransform;
                currentNetworkTransform = networkTransform;
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
            //lagMachine.Update();

            if (!IsRunning || receivedTransforms == 0)
            {
                return;
            }

            Interpolate(false);

            timeSinceCurrentTransform += Time.deltaTime;
        }

        private void Interpolate(bool extrapolate)
        {
            if (!IsRunning || receivedTransforms == 0)
            {
                return;
            }

            if (receivedTransforms < 2 || currentNetworkTransform.TimeStamp == previousNetworkTransform.TimeStamp)
            {
                ApplyLocalTransform(currentNetworkTransform);
                return;
            }

            var currentMoveStep = CalculateMoveStep(
                extrapolate ? currentNetworkTransform : previousNetworkTransform,
                timeSinceCurrentTransform
            );

            var localMoveStep = CalculateMoveStep(
                currentLocalTransform,
                Time.deltaTime
            );

            var deviationFactor = Mathf.Min(1.0f, TANK_DEVIATION_CORRECTION_FACTOR * Time.deltaTime);
            var lerpedTransform = LerpNetworkTransforms(localMoveStep, currentMoveStep, deviationFactor);

            ApplyLocalTransform(lerpedTransform);
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
                TimeStamp = origin.TimeStamp + (long)(stepTime * 1000.0f)
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
                TimeStamp = (long)(a.TimeStamp * (1.0f - factor) + b.TimeStamp * factor)
            };
        }

        private void ApplyLocalTransform(NetworkTransform appliedTransform)
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
