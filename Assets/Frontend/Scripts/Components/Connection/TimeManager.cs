using GLShared.Networking.Components;
using UnityEngine;
using Zenject;

namespace Frontend.Scripts.Components
{
    public class TimeManager : MonoBehaviour, IInitializable
    {
        [Inject] private readonly ConnectionManager connectionManager;
        [Inject] private readonly SmartFoxConnection smartFox;

        private readonly float period = 0.08f;
        private readonly int averagePingCount = 10;
        
        private float lastRequestTime = float.MaxValue;
        private float timeBeforeSync = 0;
       
        private double lastServerTime = 0;
        private double lastLocalTime = 0;
        private double averagePing = 0;
        private double[] pingValues;

        private int pingCount = 0;
        private int pingValueIndex;

        private bool isRunning = false;

        public double NetworkTime
        {
            get
            {
                // Taking server timestamp + time passed locally since the last server time received			
                return (Time.time - lastLocalTime) * 1000 + lastServerTime;
            }
        }
        public double AveragePing => averagePing;

        public void Initialize()
        {
            pingValues = new double[averagePingCount];
            pingCount = 0;
            pingValueIndex = 0;
            isRunning = true;
        }

        public double GetAveragePingAndSync(double timeValue)
        {
            double ping = (Time.time - timeBeforeSync) * 1000; //miliseconds
            CalculateAveragePing(ping);

            // Take the time passed between server sends response and we get it as half of the average ping valu
            double timePassed = averagePing / 2.0f;
            lastServerTime = timeValue + timePassed;
            lastLocalTime = Time.time;

            return averagePing;
        }

        private void Update()
        {
            if (!isRunning)
            {
                return;
            }

            if (lastRequestTime > period)
            {
                lastRequestTime = 0;
                timeBeforeSync = Time.time;
                TrySendTimeRequest();
            }
            else
            {
                lastRequestTime += Time.deltaTime;
            }
        }

        private void CalculateAveragePing(double ping)
        {
            pingValues[pingValueIndex] = ping;
            pingValueIndex++;

            if (pingValueIndex >= averagePingCount)
            {
                pingValueIndex = 0;
            }
            if (pingCount < averagePingCount)
            {
                pingCount++;
            }

            double pingSum = 0;
            for (int i = 0; i < pingCount; i++)
            {
                pingSum += pingValues[i];
            }

            averagePing = pingSum / pingCount;
        }

        private void TrySendTimeRequest()
        {
            if(smartFox.IsInitialized && smartFox.Connection.IsConnected)
            {
                connectionManager.SendRequest("getTime", null);
            }
        }

    }
}
