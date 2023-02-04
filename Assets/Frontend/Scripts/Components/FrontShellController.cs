using GLShared.General.Interfaces;
using GLShared.General.Signals;
using GLShared.Networking.Components;
using GLShared.Networking.Interfaces;
using GLShared.Networking.Models;
using UnityEngine;
using Zenject;

namespace Frontend.Scripts.Components
{
    public class FrontShellController : MonoBehaviour, IShellController, ISyncInterpolator
    {
        private const float MIN_THRESHOLD = 0.0001f;

        [Inject] private readonly SignalBus signalBus;
        [Inject] private readonly TimeManager timeManager;
        [Inject] private readonly ShellEntity shellEntity;

        private double interpolationBackTime = 200;
        private int statesCount = 0;
        private NetworkShellTransform[] bufferedStates = new NetworkShellTransform[20];

        public bool IsRunning { get; private set; } = false;
        public string OwnerUsername => shellEntity.Properties.Username;
        public float Velocity => shellEntity.CurrentNetworkTransform.CurrentSpeed;


        public void Initialize()
        {
            signalBus.Subscribe<ShellSignals.OnShellDestroyed>(OnShellDestroyed);
            IsRunning = true;
            statesCount = 0;
        }

        public void Dispose()
        {
        }

        public void ProcessCurrentNetworkTransform(INetworkTransform nTransform)
        {
            if (!IsRunning)
            {
                return;
            }

            var networkTransform = (NetworkShellTransform)nTransform;

            for (int i = bufferedStates.Length - 1; i >= 1; i--)
            {
                bufferedStates[i] = bufferedStates[i - 1];
            }

            bufferedStates[0] = networkTransform;
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

                        transform.SetPositionAndRotation(Vector3.Lerp(lhs.Position, rhs.Position, t), Quaternion.Slerp(lhs.Rotation, rhs.Rotation, t));
                        return;
                    }
                }
            }
            else
            {
                transform.SetPositionAndRotation(bufferedStates[0].Position, bufferedStates[0].Rotation);
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

        private void OnShellDestroyed(ShellSignals.OnShellDestroyed OnShellDestroyed)
        {
            if (OnShellDestroyed.ShellSceneId == shellEntity.Properties.ShellSceneIdentifier)
            {
                Destroy(gameObject);
            }
        }
    }
}

