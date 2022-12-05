using Frontend.Scripts.Interfaces;
using Sfs2X.Bitswarm;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
using System.Linq;
using GLShared.General.Interfaces;
using Frontend.Scripts.Components;

namespace Frontend.Scripts.Models
{
    public abstract class WheelRepositionBase : MonoBehaviour, IWheelReposition, IInitializable
    {
        [Inject] protected readonly IVehicleController controller;
        [Inject] protected readonly IPlayerInputProvider inputProvider;
        [Inject] protected readonly VehicleModelEffects vehicleModelEffects;
        [Inject(Id = "mainRig")] protected readonly Rigidbody rig;

        [SerializeField] protected float visualElementsMovementSpeed = 50f;

        protected float repositionSpeed = 0;
        public float RepositionSpeed => repositionSpeed;
        public float VisualElementsMovementSpeed => visualElementsMovementSpeed;
        public virtual void Initialize()
        {
        }

        protected virtual void FixedUpdate()
        {
            if (vehicleModelEffects.IsInsideCameraView)
            {
                repositionSpeed = visualElementsMovementSpeed * Mathf.Max(0.4f, controller.CurrentSpeedRatio);
            }
        }
        public virtual void RotateWheel(float verticalDir, Vector3 rotateAroundAxis, Transform tireTransform, UTAxlePair pair)
        {
        }

        public virtual void DummiesMovement(Transform tireTransform, UTAxlePair pair, Vector3 finalWheelPosition, float movementSpeed)
        {
        }

    }
}
