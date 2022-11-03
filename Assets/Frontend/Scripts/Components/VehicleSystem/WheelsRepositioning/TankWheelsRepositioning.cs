using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Frontend.Scripts.Models;
using System;

namespace Frontend.Scripts.Components
{
    public class TankWheelsRepositioning : WheelRepositionBase
    {
        public override void RotateWheels(float verticalDir, Vector3 rotateAroundAxis, Transform tireTransform, UTAxlePair pair, out float currentToMaxRatio)
        {
            base.RotateWheels(verticalDir, rotateAroundAxis, tireTransform, pair, out _);
            currentToMaxRatio = 0.5f;
            float rawHorizontal = inputProvider.RawHorizontal;
            if (inputProvider.RawVertical == 0 && rawHorizontal != 0)//stationary tank rotating
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
                    if (rawHorizontal != 0 && (int)pair.Axis == rawHorizontal)//moving fwd/bwd and turning in the same time
                    {
                        currentToMaxRatio /= 2f; //whenever we go forward we want one side of wheels to move slower
                    }
                    pair.RotationalPartOfTire.RotateAround(tireTransform.position, rotateAroundAxis, verticalDir * (currentToMaxRatio * 1300f) * Time.deltaTime);
                }
            }
            
        }

        public override void TrackMovement(Transform tireTransform, UTAxlePair pair, Vector3 finalWheelPosition)
        {
            base.TrackMovement(tireTransform, pair, finalWheelPosition);
            var dummyPair = pair.WheelDummyPair;
            if (dummyPair.trackDummy != null)
            {
                Vector3 desiredPos = finalWheelPosition + new Vector3(0, dummyPair.dummyOffsetY, 0);
                dummyPair.trackDummy.position = desiredPos;
            }
        }

        private float GetTankWheelRotationInputDir(float rawHorizontal, UTAxlePair pair)
        {
            return pair.Axis == GLShared.General.Enums.DriveAxisSite.Left ? -rawHorizontal : rawHorizontal;
        }
    }
}
