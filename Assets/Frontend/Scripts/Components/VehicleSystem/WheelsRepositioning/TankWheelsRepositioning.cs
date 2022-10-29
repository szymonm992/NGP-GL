using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Frontend.Scripts.Models;

namespace Frontend.Scripts.Components
{
    public class TankWheelsRepositioning : WheelRepositionBase
    {
        public override void RotateWheels(float verticalDir, Vector3 rotateAroundAxis, Transform tireTransform, UTAxlePair pair, out float currentToMaxRatio)
        {
            base.RotateWheels(verticalDir, rotateAroundAxis, tireTransform, pair, out _);
            currentToMaxRatio = 0.5f;
            float rawHorizontal = inputProvider.RawHorizontal;
            if (inputProvider.RawVertical == 0 && rawHorizontal != 0)
            {
                currentToMaxRatio = 0.3f;
                pair.RotationalPartOfTire.RotateAround(tireTransform.position, rotateAroundAxis, GetTankWheelRotationInputDir(rawHorizontal, pair) * (currentToMaxRatio * 1300f) * Time.deltaTime);
            }
            else
            {
                float currentMaxSpeed = controller.GetCurrentMaxSpeed();
               
                if (currentMaxSpeed != 0)
                {
                    currentToMaxRatio = controller.CurrentSpeed / currentMaxSpeed;
                    if (rawHorizontal != 0 && (int)pair.Axis == rawHorizontal)
                    {
                        currentToMaxRatio /= 2f;
                    }
                    pair.RotationalPartOfTire.RotateAround(tireTransform.position, rotateAroundAxis, verticalDir * (currentToMaxRatio * 1300f) * Time.deltaTime);
                }
            }
            
        }

        private float GetTankWheelRotationInputDir(float rawHorizontal, UTAxlePair pair)
        {
            return pair.Axis == GLShared.General.Enums.DriveAxisSite.Left ? -rawHorizontal : rawHorizontal;
        }

    }
}
