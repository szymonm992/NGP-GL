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

        public override void RotateWheels(float verticalDir, Vector3 rotateAroundAxis, Transform tireTransform, UTAxlePair pair, out float currentToMaxRatio)
        {
            base.RotateWheels(verticalDir, rotateAroundAxis, tireTransform, pair, out _);
            float currentMaxSpeed = controller.GetCurrentMaxSpeed();
            currentToMaxRatio = 0.5f;
            if (currentMaxSpeed != 0)
            {
                currentToMaxRatio = controller.CurrentSpeedRatio;
                pair.RotationalPartOfTire.RotateAround(tireTransform.position, rotateAroundAxis, verticalDir * (currentToMaxRatio * 1300f) * Time.deltaTime);
            }
        }
    }
}
