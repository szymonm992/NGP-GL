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
        private const float SHELL_VISUALS_TOGGLE_DISTANCE = 10f;

        [Inject] private readonly SignalBus signalBus;
        [Inject] private readonly TimeManager timeManager;
        [Inject] private readonly ShellEntity shellEntity;

        [SerializeField] private GameObject visualsParent;

        private bool enabledVisuals = false;
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

        private void FixedUpdate()
        {
            if (!IsRunning || statesCount == 0)
            {
                return;
            }

            SelectInterpolationTime(timeManager.AveragePing);
            Interpolate(timeManager.NetworkTime);
            CheckVisuals();
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
            int rightIndex = 0;

            // find the right index of the buffer
            for (int i = 0; i < statesCount; i++)
            {
                if (bufferedStates[i].TimeStamp <= interpolationTime || i == statesCount - 1)
                {
                    rightIndex = i;
                    break;
                }
            }

            int leftIndex = Mathf.Max(rightIndex - 1, 0);
            var rightState = bufferedStates[rightIndex];
            var leftState = bufferedStates[leftIndex];

            double length = rightState.TimeStamp - leftState.TimeStamp;
            float t = 0.0f;

            // perform interpolation only if length is greater than the minimum threshold
            if (length > MIN_THRESHOLD)
            {
                t = (float)((interpolationTime - leftState.TimeStamp) / length);
            }

            transform.SetPositionAndRotation(Vector3.Lerp(leftState.Position, rightState.Position, t), Quaternion.Slerp(leftState.Rotation, rightState.Rotation, t));
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

        private void CheckVisuals()
        {
            if (!enabledVisuals)
            {
                if (Vector3.Distance(shellEntity.Properties.SpawnPosition, transform.position) >= SHELL_VISUALS_TOGGLE_DISTANCE)
                {
                    visualsParent.ToggleGameObjectIfActive(true);
                    enabledVisuals = true;
                }
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

