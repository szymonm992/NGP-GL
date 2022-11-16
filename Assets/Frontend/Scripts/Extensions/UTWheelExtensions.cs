using Frontend.Scripts.Components;
using Frontend.Scripts.Interfaces;
using GLShared.General.Enums;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Frontend.Scripts.Extensions
{
    public static class UTWheelExtensions 
    {
        public static Vector3 ReturnWheelPoint(this IPhysicsWheel wheel, ForceApplyPoint forceApplyPoint)
        {
            if (forceApplyPoint == ForceApplyPoint.WheelConstraintUpperPoint)
            {
                return wheel.UpperConstraintPoint;
            }
            else if (forceApplyPoint == ForceApplyPoint.WheelConstraintLowerPoint)
            {
                return wheel.LowerConstraintPoint;
            }
            else
            {
                return wheel.HitInfo.Point;
            }

        }
    }
}
