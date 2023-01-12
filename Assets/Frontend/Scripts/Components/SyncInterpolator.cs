using GLShared.General.Interfaces;
using GLShared.General.Signals;
using GLShared.Networking.Components;
using GLShared.Networking.Interfaces;
using GLShared.Networking.Models;
using UnityEngine;
using UnityEngine.UIElements;
using Zenject;

namespace Frontend.Scripts.Components
{
    public class SyncInterpolator : MonoBehaviour, IInitializable, ISyncInterpolator
    {
        private const float MIN_THRESHOLD = 0.0001f;

        [Inject] private readonly SignalBus signalBus;
        [Inject] private readonly TimeManager timeManager;
        [Inject] private readonly Speedometer speedometer;
        [Inject(Optional = true)] private readonly FrontTurretController turretController;
        [Inject(Source = InjectSources.Local)] private readonly PlayerEntity playerEntity;

        private double interpolationBackTime = 200;
        private int statesCount = 0;
        private NetworkTransform[] bufferedStates = new NetworkTransform[20];

        public bool IsRunning { get; private set; } = false;

        public void Initialize()
        {
            signalBus.Subscribe<PlayerSignals.OnPlayerInitialized>(OnPlayerInitialized);
            statesCount = 0;
        }

        public void ProcessCurrentNetworkTransform(NetworkTransform nTransform)
        {
            if (!IsRunning)
            {
                return;
            }

            for (int i = bufferedStates.Length - 1; i >= 1; i--)
            {
                bufferedStates[i] = bufferedStates[i - 1];
            }

            bufferedStates[0] = nTransform;
            statesCount = Mathf.Min(statesCount + 1, bufferedStates.Length);
        }

        private void Update()
        {
            if (!IsRunning || statesCount == 0)
            {
                return;
            }

            SelectInterpolationTime(timeManager.AveragePing);
            Interpolate(timeManager.NetworkTime);
        }


        private void Interpolate(double currentTime)
        {
            if (!IsRunning)
            {
                return;
            }

            if (statesCount == 0)
            {
                return;
            }

            double interpolationTime = currentTime - interpolationBackTime;

            if (bufferedStates[0].TimeStamp > interpolationTime)
            {
                for (int i = 0; i < statesCount; i++)
                {
                    if (bufferedStates[i].TimeStamp <= interpolationTime || i == statesCount - 1)
                    {
                        var rhs = bufferedStates[Mathf.Max(i - 1, 0)];
                        var lhs = bufferedStates[i];
                        double length = rhs.TimeStamp - lhs.TimeStamp;
                        float t = 0.0f;
                        if (length > MIN_THRESHOLD)
                        {
                            t = (float)((interpolationTime - lhs.TimeStamp) / length);
                        }
                        speedometer.SetSpeedometr(lhs.CurrentSpeed);
                        transform.position = Vector3.Lerp(lhs.Position, rhs.Position, t);
                        transform.rotation = Quaternion.Slerp(lhs.Rotation, rhs.Rotation, t);
                        turretController.SetTurretAndGunRotation(rhs.TurretAngleY, rhs.GunAngleX);
                        return;
                    }
                }
            }
            else
            {
                transform.position = bufferedStates[0].Position;
                transform.rotation = bufferedStates[0].Rotation;
                turretController.SetTurretAndGunRotation(bufferedStates[0].TurretAngleY, bufferedStates[0].GunAngleX);
                speedometer.SetSpeedometr(bufferedStates[0].CurrentSpeed);
                
            }
        }


        private void SelectInterpolationTime(double averagePing)
        {
            if (averagePing < 50)
            {
                interpolationBackTime = 50;
            }
            else if (averagePing < 100)
            {
                interpolationBackTime = 100;
            }
            else if (averagePing < 200)
            {
                interpolationBackTime = 200;
            }
            else if (averagePing < 400)
            {
                interpolationBackTime = 400;
            }
            else if (averagePing < 600)
            {
                interpolationBackTime = 600;
            }
            else
            {
                interpolationBackTime = 1000;
            }
        }

        private void OnPlayerInitialized(PlayerSignals.OnPlayerInitialized signal)
        {
            IsRunning = true;
        }
    }
}
