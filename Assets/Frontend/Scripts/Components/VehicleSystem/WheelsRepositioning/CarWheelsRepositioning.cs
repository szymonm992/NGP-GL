using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Frontend.Scripts.Models;
using Sfs2X.Bitswarm;
using Frontend.Scripts.Interfaces;
using Zenject;

namespace Frontend.Scripts.Components
{
    public class CarWheelsRepositioning : WheelRepositionBase
    {

        public override void RotateWheel(float verticalDir, Vector3 rotateAroundAxis, Transform tireTransform, UTAxlePair pair)
        {
            base.RotateWheel(verticalDir, rotateAroundAxis, tireTransform, pair);
            float speed = controller.CurrentSpeed;
            pair.RotationalPartOfTire.RotateAround(tireTransform.position, rotateAroundAxis, verticalDir * (speed * 25f) * Time.deltaTime);
        }
    }
}
