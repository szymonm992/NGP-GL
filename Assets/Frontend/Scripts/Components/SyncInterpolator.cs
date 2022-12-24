using GLShared.General.Interfaces;
using GLShared.Networking.Components;
using GLShared.Networking.Models;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Zenject;

namespace Frontend.Scripts.Components
{
    public class SyncInterpolator : MonoBehaviour, IInitializable
    {
        private const float MIN_THRESHOLD = 0.0001f;

        [Inject] private readonly TimeManager timeManager;
        [Inject(Optional = true)] private readonly Speedometer speedometer;

        private double interpolationBackTime = 200;
        NetworkTransform[] bufferedStates = new NetworkTransform[20];
        int statesCount = 0;

        public bool IsRunning { get; set; } = false;

        public void Initialize()
        {
        }

        public void StartReceiving()
        {
            IsRunning = true;
        }

        public void ReceivedTransform(NetworkTransform nTransform)
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

            UpdateValues();

            double currentTime = timeManager.NetworkTime;
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

                        transform.position = Vector3.Lerp(lhs.Position, rhs.Position, t);
                        transform.eulerAngles = Vector3.Lerp(lhs.EulerAngles, rhs.EulerAngles, t);

                        if (speedometer != null)
                        {
                            speedometer.SetSpeedometr(lhs.CurrentSpeed);
                        }
                        return;
                    }
                }
            }
            else
            {
                transform.position = bufferedStates[0].Position;
                transform.eulerAngles = bufferedStates[0].EulerAngles;
                if (speedometer != null)
                {
                    speedometer.SetSpeedometr(bufferedStates[0].CurrentSpeed);
                }
            }

        }

        private void UpdateValues()
        {
            double ping = timeManager.AveragePing;

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

        
    }
}
