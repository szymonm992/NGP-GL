using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Frontend.Scripts.Models;
using Frontend.Scripts.Interfaces;
using GLShared.General.Enums;
using System.Linq;
using Zenject;

namespace Frontend.Scripts.Components
{
    public class TankWheelsRepositioning : WheelRepositionBase
    {
        [System.Serializable]
        public struct TrackProperties
        {
            public Renderer trackObject;
            public DriveAxisSite trackAxis;
        }

        [Inject] private readonly UTTankSteering tankSteering;
        
        [SerializeField] private TrackProperties[] tracksList;

        private IEnumerable<TrackProperties> leftTracks;
        private IEnumerable<TrackProperties> rightTracks;
        private float trackTurningRotationSpeed;

        public override void Initialize()
        {
            base.Initialize();
            if (tracksList != null && tracksList.Any())
            {
                leftTracks = tracksList.Where(track => track.trackAxis == DriveAxisSite.Left);
                rightTracks = tracksList.Where(track => track.trackAxis == DriveAxisSite.Right);
                trackTurningRotationSpeed = tankSteering.SteerForce / 10f;
            }
        }
        public override void RotateWheels(float verticalDir, Vector3 rotateAroundAxis, Transform tireTransform, UTAxlePair pair, out float currentToMaxRatio)
        {
            base.RotateWheels(verticalDir, rotateAroundAxis, tireTransform, pair, out _);
            currentToMaxRatio = 0.5f;
            float rawHorizontal = inputProvider.RawHorizontal;
            if (inputProvider.RawVertical == 0 && rawHorizontal != 0)//stationary tank rotating
            {
                currentToMaxRatio = 0.3f;
                pair.RotationalPartOfTire.RotateAround(tireTransform.position, rotateAroundAxis, GetTankWheelRotationInputDir(rawHorizontal, pair.Axis) * (currentToMaxRatio * 1300f) * Time.deltaTime);
            }
            else
            {
                float currentMaxSpeed = controller.GetCurrentMaxSpeed();

                if (currentMaxSpeed != 0)
                {
                    currentToMaxRatio = controller.CurrentSpeedRatio;
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

            float currentMaxSpeed = controller.GetCurrentMaxSpeed();
            if (tracksList != null && tracksList.Any() && currentMaxSpeed != 0)
            {
                var leftAndRight = GetTrackSideMultipliers();

                float l = leftAndRight.Item1;
                float r = leftAndRight.Item2;

                float trackRotSpeed = 30f;
                float offset;

                if (l == r)
                {
                    if (controller.CurrentSpeedRatio < (currentMaxSpeed / 1.5f))
                    {
                        offset = (trackRotSpeed * controller.CurrentSpeedRatio) * Time.deltaTime;
                    }
                    else
                    {
                        offset = (currentMaxSpeed / 1.5f) * trackRotSpeed * Time.deltaTime;
                    }
                }
                else
                { 
                    offset = trackTurningRotationSpeed * Time.deltaTime; 
                }


                if (leftTracks.Any())
                {
                    foreach (var rend in leftTracks)
                    {
                        var materials = rend.trackObject.materials;
                        float currentTextureOffset = materials[0].GetTextureOffset("_BaseMap").y;

                        materials[0].SetTextureOffset("_BaseMap", new Vector2(0, currentTextureOffset - (offset / 70f) * l));
                        rend.trackObject.materials = materials;
                    }
                }

                if (rightTracks.Any())
                {
                    foreach (var rend in rightTracks)
                    {
                        var materials = rend.trackObject.materials;
                        float currentTextureOffset = materials[0].GetTextureOffset("_BaseMap").y;

                        materials[0].SetTextureOffset("_BaseMap", new Vector2(0, currentTextureOffset - (offset / 70f) * r));
                        rend.trackObject.materials = materials;
                    }
                }
            }
        }

        private (float, float) GetTrackSideMultipliers()
        {
            if(inputProvider.AbsoluteHorizontal > 0)
            {
                return (inputProvider.SignedHorizontal, -inputProvider.SignedHorizontal);
            }
            else
            {
                return (inputProvider.SignedVertical, inputProvider.SignedVertical);
            }
        }

        private float GetTankWheelRotationInputDir(float rawHorizontal, DriveAxisSite driveAxis)
        {
            return driveAxis == DriveAxisSite.Left ? -rawHorizontal : rawHorizontal;
        }
    }
}
