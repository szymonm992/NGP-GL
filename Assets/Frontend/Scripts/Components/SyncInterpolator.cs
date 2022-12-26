using GLShared.General.Interfaces;
using GLShared.General.Signals;
using GLShared.Networking.Components;
using GLShared.Networking.Interfaces;
using GLShared.Networking.Models;
using System.Collections;
using System.Collections.Generic;
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
        [Inject(Source = InjectSources.Local)] private readonly PlayerEntity playerEntity;

        private double interpolationBackTime = 200;
        private int statesCount = 0;
        NetworkTransform[] bufferedStates = new NetworkTransform[20];

        public bool IsRunning { get; private set; } = false;

        public void Initialize()
        {
            signalBus.Subscribe<PlayerSignals.OnPlayerInitialized>(OnPlayerInitialized);
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

            var currentTime = timeManager.NetworkTime;
            var interpolationTime = currentTime - interpolationBackTime;
            var firstBufferedState = bufferedStates[0];

            if (firstBufferedState.TimeStamp > interpolationTime)
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

                        transform.position = Vector3.Lerp(lhs.Position, rhs.Position, t);
                        transform.eulerAngles = Vector3.Lerp(lhs.EulerAngles, rhs.EulerAngles, t);

                        if (playerEntity.IsLocalPlayer)
                        {
                            speedometer.SetSpeedometr(lhs.CurrentSpeed);
                        }
                        return;
                    }
                }
            }
            else
            {
                transform.position = firstBufferedState.Position;
                transform.eulerAngles = firstBufferedState.EulerAngles;
                if (playerEntity.IsLocalPlayer)
                {
                    speedometer.SetSpeedometr(firstBufferedState.CurrentSpeed);
                }
            }

        }

        private void SelectInterpolationTime(double ping)
        {
            if (ping < 50)
            {
                interpolationBackTime = 50;
            }
            else if (ping < 100)
            {
                interpolationBackTime = 100;
            }
            else if (ping < 200)
            {
                interpolationBackTime = 200;
            }
            else if (ping < 400)
            {
                interpolationBackTime = 400;
            }
            else if (ping < 600)
            {
                interpolationBackTime = 600;
            }
            else
            {
                interpolationBackTime = 1000;
            }
        }

        private void OnPlayerInitialized(PlayerSignals.OnPlayerInitialized OnPlayerInitialized)
        {
            if(playerEntity.Properties.User.Name == OnPlayerInitialized.PlayerProperties.User.Name)
            {
                IsRunning = true;
            }
        }
    }
}
