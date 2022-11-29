using Frontend.Scripts.Interfaces;
using Sfs2X.Bitswarm;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
using System.Linq;
using GLShared.General.Interfaces;

namespace Frontend.Scripts.Models
{
    public abstract class WheelRepositionBase : MonoBehaviour, IWheelReposition, IInitializable
    {
        [Inject] protected readonly IVehicleController controller;
        [Inject] protected readonly IPlayerInputProvider inputProvider;
        [Inject(Id = "mainRig")] protected readonly Rigidbody rig;

        public virtual float RepositionSpeed => 0;

        public virtual void Initialize()
        {
        }

        public virtual void RotateWheel(float verticalDir, Vector3 rotateAroundAxis, Transform tireTransform, UTAxlePair pair)
        {
        }

        public virtual void DummiesMovement(Transform tireTransform, UTAxlePair pair, Vector3 finalWheelPosition, float movementSpeed)
        {
        }

    }
}
