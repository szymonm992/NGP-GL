using Frontend.Scripts.Enums;
using Frontend.Scripts.Models;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Frontend.Scripts.Components
{
    public class UTAxle : MonoBehaviour
    {
        [SerializeField] private UTAxlePair[] wheelPairs;
        [SerializeField] private bool canDrive;
        [SerializeField] private bool canSteer;

        public UTAxleDebug debugSettings = new UTAxleDebug()
        {
            DrawGizmos = true,
            DrawAxleCenter = true,
            DrawAxlePipes = true,
            DrawMode = UTDebugMode.All
        };

        public UTAxlePair[] WheelPairs => wheelPairs;
        public bool CanDrive => canDrive;
        public bool CanSteer => canSteer;

        public bool HasAnyWheelPair => wheelPairs.Any();

        public IEnumerable<UTWheel> GetGroundedWheels()
        {
            return wheelPairs.Where(pair => pair.Wheel.IsGrounded == true).Select(pair => pair.Wheel).ToArray();
        } 

        public IEnumerable<UTWheel> GetAllWheels()
        {
            return wheelPairs.Select(pair => pair.Wheel).ToArray();
        }

        public void SetSteerAngle(float angleLeftAxis, float angleRightAxis)
        {
            foreach(var pair in wheelPairs)
            {
                pair.Wheel.SteerAngle = pair.Axis == DriveAxisSite.Left ? angleLeftAxis : angleRightAxis;
            }
        }

        private void Update()
        {
            foreach (var pair in wheelPairs)
            {
                if (pair.TireModel != null)
                {
                    RepositionTireModel(pair);
                }
            }
        }

        private void RepositionTireModel(UTAxlePair pair)
        {
            pair.TireModel.SetPositionAndRotation(pair.Wheel.TireWorldPosition, pair.Wheel.transform.rotation);
        }

        private void OnDrawGizmos()
        {
            bool drawCurrently = (debugSettings.DrawGizmos) && (debugSettings.DrawMode == UTDebugMode.All)
               || (debugSettings.DrawMode == UTDebugMode.EditorOnly && !Application.isPlaying)
               || (debugSettings.DrawMode == UTDebugMode.PlaymodeOnly && Application.isPlaying);

            if (drawCurrently)
            {
                Gizmos.color = Color.white;
                if(debugSettings.DrawAxleCenter)
                {
                    Gizmos.DrawSphere(transform.position, .11f);
                }
                
                if(debugSettings.DrawAxlePipes)
                {
                    foreach (var pair in wheelPairs)
                    {
                        Handles.color = Color.white;
                        Handles.DrawLine(pair.Wheel.transform.position, transform.position, 1.2f);
                    }
                }
                
            }
        }
    }
}
