using Frontend.Scripts.Interfaces;
using UnityEngine;
using Zenject;
using GLShared.General.Interfaces;

namespace Frontend.Scripts.Components
{
    public class UTAckermann : MonoBehaviour, IVehicleSteering
    {
        [Inject] private readonly IVehicleController suspensionController;
        [Inject] private readonly IPlayerInputProvider inputProvider;

        [SerializeField] private float wheelBase;
        [SerializeField] private float rearTrack;
        [SerializeField] private AnimationCurve turnRadiusCurve;

        private float ackermannAngleRight;
        private float ackermannAngleLeft;
        private float steerInput;
        private float currentTurnRadius;

        public void SetSteeringInput(float input)
        {
            steerInput = input;
        }

        private void Update()
        {
            if (suspensionController != null)
            {
                SetSteeringInput(inputProvider.Horizontal);
            }
        }

        private void FixedUpdate()
        {
            currentTurnRadius = turnRadiusCurve.Evaluate(suspensionController.CurrentSpeed);

            if (steerInput > 0)
            {
                ackermannAngleLeft = Mathf.Rad2Deg * Mathf.Atan(wheelBase / (currentTurnRadius + (rearTrack / 2))) * steerInput;
                ackermannAngleRight = Mathf.Rad2Deg * Mathf.Atan(wheelBase / (currentTurnRadius - (rearTrack / 2))) * steerInput;
            }
            else if (steerInput < 0)
            {
                ackermannAngleLeft = Mathf.Rad2Deg * Mathf.Atan(wheelBase / (currentTurnRadius - (rearTrack / 2))) * steerInput;
                ackermannAngleRight = Mathf.Rad2Deg * Mathf.Atan(wheelBase / (currentTurnRadius + (rearTrack / 2))) * steerInput;
            }
            else
            {
                ackermannAngleLeft = 0;
                ackermannAngleRight = 0;
            }

            if (suspensionController.HasAnyWheels)
            {
                Debug.Log("bbb");
                foreach (var axle in suspensionController.AllAxles)
                {
                    if (axle.CanSteer)
                    {
                        int multiplier = axle.InvertSteer ? -1 : 1;
                        axle.SetSteerAngle(ackermannAngleLeft * multiplier, ackermannAngleRight * multiplier);
                    }
                }
            }
            else
            {
                Debug.Log("aaa" );
            }
        }
    }
}
