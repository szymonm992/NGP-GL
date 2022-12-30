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
        private const float POSITION_DIFFERENCE_THRESHOLD = 0.1f;
        private const float ROTATION_DIFFERENCE_THRESHOLD = 5f;

        [Inject] private readonly SignalBus signalBus;
        [Inject] private readonly TimeManager timeManager;
        [Inject] private readonly Speedometer speedometer;
        [Inject(Source = InjectSources.Local)] private readonly PlayerEntity playerEntity;

        private double interpolationBackTime = 200;
        private int statesCount = 0;
        private NetworkTransform[] bufferedStates = new NetworkTransform[20];
        private NetworkTransform targetState = new NetworkTransform();
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

            // Store the incoming NetworkTransform in a circular fashion
            int index = (statesCount % bufferedStates.Length);
            bufferedStates[index] = nTransform;
            statesCount++;
        }

        private void Update()
        {
            if (!IsRunning || statesCount == 0)
            {
                return;
            }

            SelectInterpolationTime(timeManager.AveragePing);

            var currentTime = timeManager.NetworkTime;
            if (ShouldExtrapolate(currentTime, out targetState))
            {
                Extrapolate(currentTime, targetState);
            }
            else
            {
                Interpolate(currentTime);
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
            else
            {
                interpolationBackTime = 800;
            }
        }

        private void Interpolate(double currentTime)
        {
            var interpolationTime = currentTime - interpolationBackTime;
            var firstBufferedState = bufferedStates[(statesCount - 1) % bufferedStates.Length];

            if (firstBufferedState.TimeStamp > interpolationTime)
            {
                for (int i = 0; i < statesCount; i++)
                {
                    int index = (statesCount - 1 - i) % bufferedStates.Length;
                    if (bufferedStates[index].TimeStamp <= interpolationTime || i == statesCount - 1)
                    {
                        var rhs = bufferedStates[(index - 1 + bufferedStates.Length) % bufferedStates.Length];
                        var lhs = bufferedStates[index];
                        var length = rhs.TimeStamp - lhs.TimeStamp;

                        var t = 0.0f;
                        if (length > MIN_THRESHOLD)
                        {
                            t = length > MIN_THRESHOLD ? (float)((interpolationTime - lhs.TimeStamp) / length) : 0.0f;
                        }

                        transform.SetPositionAndRotation(Vector3.MoveTowards(lhs.Position, rhs.Position, t),
                            Quaternion.RotateTowards(lhs.Rotation, rhs.Rotation, t));

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
                transform.eulerAngles = firstBufferedState.Rotation.eulerAngles;

                if (playerEntity.IsLocalPlayer)
                {
                    speedometer.SetSpeedometr(firstBufferedState.CurrentSpeed);
                }
            }
        }

        private void Extrapolate(double currentTime, NetworkTransform targetState)
        {
            double length = targetState.TimeStamp - bufferedStates[0].TimeStamp;
            float t = 0.0f;
            if (length > MIN_THRESHOLD)
            {
                t = (float)((currentTime - bufferedStates[0].TimeStamp) / length);
            }

            transform.position = Vector3.MoveTowards(bufferedStates[0].Position, targetState.Position, t);
            transform.rotation = Quaternion.RotateTowards(bufferedStates[0].Rotation, targetState.Rotation, t);
            if (playerEntity.IsLocalPlayer)
            {
                speedometer.SetSpeedometr(bufferedStates[0].CurrentSpeed);
            }
        }

        private void OnPlayerInitialized(PlayerSignals.OnPlayerInitialized signal)
        {
            IsRunning = true;
        }

        const float FORWARD_DIFFERENCE_THRESHOLD = 0.9f;
        private const float EPSILON = 0.001f;

        private bool ShouldExtrapolate(double currentTime, out NetworkTransform targetState)
        {
            targetState = null;
            var firstBufferedState = bufferedStates[0];
            if (firstBufferedState.TimeStamp >= currentTime)
            {
                return false;
            }

            for (int i = 0; i < statesCount; i++)
            {
                if (bufferedStates[i].TimeStamp <= currentTime || i == statesCount - 1)
                {
                    if (i == 0)
                    {
                        return false;
                    }

                    // Calculate the difference between the current forward direction and the previous forward direction
                    var currentForward = transform.forward;
                    var previousForward = bufferedStates[i - 1].Rotation * Vector3.forward;
                    var forwardDifference = Vector3.Angle(currentForward, previousForward);

                    // Check if the difference is greater than the threshold and if the position or rotation has changed significantly
                    if (forwardDifference > FORWARD_DIFFERENCE_THRESHOLD + EPSILON && bufferedStates[i - 1].HasChanged(bufferedStates[i], POSITION_DIFFERENCE_THRESHOLD, ROTATION_DIFFERENCE_THRESHOLD))
                    {
                        targetState = bufferedStates[i];
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            return false;
        }
    }
}
