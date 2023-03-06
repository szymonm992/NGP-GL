using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Frontend.Scripts.Test
{
    public class LagMachine
    {
        public float MinLength { get; set; }
        public float MaxLength { get; set; }
        public float MinSpike { get; set; }
        public float MaxSpike { get; set; }
        public float PacketMissChance { get; set; }

        private List<(System.Action Action, float ExecutionTime)> queuedActions;
        private float currentLagSpike;
        private float nextSpikeTime;

        public LagMachine()
        {
            queuedActions = new List<(System.Action Action, float ExecutionTime)>();
            nextSpikeTime = 0.0f;
        }

        public void Update()
        {
            if (nextSpikeTime <= Time.time)
            {
                nextSpikeTime = Time.time + Random.Range(MinLength, MaxLength);
                currentLagSpike = Random.Range(MinSpike, MaxSpike);
            }

            var remainingActions = new List<(System.Action Action, float ExecutionTime)>();
            foreach(var action in queuedActions)
            {
                if(action.ExecutionTime <= Time.time)
                {
                    action.Action.Invoke();
                }
                else
                {
                    remainingActions.Add(action);
                }
            }

            queuedActions = remainingActions;
        }

        public void InvokeLagged(System.Action action)
        {
            if (Random.Range(0.0f, 1.0f) < PacketMissChance) return;

            float executionTime = Time.time + currentLagSpike;

            queuedActions.Add((action, executionTime));
        }
    }
}