using GLShared.General.Interfaces;
using GLShared.General.Signals;
using GLShared.Networking.Components;
using GLShared.Networking.Interfaces;
using GLShared.Networking.Models;
using UnityEngine;
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
            Interpolate(currentTime);
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
                            t = (float)((interpolationTime - lhs.TimeStamp) / length);
                            if (t > 1.0f)
                            {
                                t = 1.0f;
                            }
                        }

                        // Calculate spline interpolation using the three most recent network transforms
                        Vector3 p0 = lhs.Position;
                        Vector3 p1 = rhs.Position;
                        Vector3 p2 = rhs.Position;
                        Vector3 p3 = rhs.Position;
                        if (statesCount > 2)
                        {
                            p2 = bufferedStates[(index - 1 + bufferedStates.Length) % bufferedStates.Length].Position;
                        }
                        if (statesCount > 3)
                        {
                            p3 = bufferedStates[(index - 2 + bufferedStates.Length) % bufferedStates.Length].Position;
                        }

                        float t2 = t * t;
                        float t3 = t2 * t;
                        Vector3 pos = InterpolateSpline(p0, p1, p2, p3, t);

                        transform.position = pos;
                        transform.rotation = Quaternion.Slerp(lhs.Rotation, rhs.Rotation, t);

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
                transform.rotation = firstBufferedState.Rotation;

                if (playerEntity.IsLocalPlayer)
                {
                    speedometer.SetSpeedometr(firstBufferedState.CurrentSpeed);
                }
            }
        }

        private Vector3 InterpolateSpline(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
        {
            float t2 = t * t;
            float t3 = t2 * t;
            return 0.5f * ((2.0f * p1) + (-p0 + p2) * t + (2.0f * p0 - 5.0f * p1 + 4.0f * p2 - p3) * t2 + (-p0 + 3.0f * p1 - 3.0f * p2 + p3) * t3);
        }


        private void SelectInterpolationTime(double averagePing)
        {
            interpolationBackTime = averagePing / 2;
        }

        private void OnPlayerInitialized(PlayerSignals.OnPlayerInitialized signal)
        {
            IsRunning = true;
        }
    }
}
