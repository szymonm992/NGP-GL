using Frontend.Scripts.Interfaces;
using Frontend.Scripts.Models;
using GLShared.General.Interfaces;
using GLShared.General.Models;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

namespace Frontend.Scripts.Components
{
    public class UTIdlerAxle : UTFrontAxleBase
    {
        private void FixedUpdate()
        {
            if (!wheelPairs.Any() || controller == null || controller.IsUpsideDown)
            {
                return;
            }

            if (repositionVisuals)
            {
                foreach (var pair in wheelPairs)
                {
                    var newPair = (UTAxlePair)pair;
                    if (newPair.TireModel != null)
                    {
                        RepositionTireModel(newPair);
                    }
                }
            }
        }


        protected override void RepositionTireModel(UTAxlePair pair)
        {
            var tireTransform = pair.VisualPartOfTire;

            if (controller.CurrentSpeed != 0)
            {
                float dir = -inputProvider.LastVerticalInput;
                Vector3 rotateAroundAxis = -tireTransform.right;
                wheelReposition.RotateWheel(dir, rotateAroundAxis, tireTransform, pair);
            }

            if (canSteer)
            {
                tireTransform.localRotation = Quaternion.Euler(tireTransform.localRotation.eulerAngles.x, pair.Wheel.SteerAngle, tireTransform.localRotation.eulerAngles.z);
            }
        }
    }
}
