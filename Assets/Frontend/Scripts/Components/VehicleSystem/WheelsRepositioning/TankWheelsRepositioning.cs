using System.Collections.Generic;
using UnityEngine;
using Frontend.Scripts.Models;
using GLShared.General.Enums;
using System.Linq;
using Zenject;
using GLShared.General.Models;
using GLShared.General.Components;

namespace Frontend.Scripts.Components
{
    public class TankWheelsRepositioning : WheelRepositionBase
    {

        [System.Serializable]
        public struct TrackProperties
        {
            public Renderer trackObject;
            public DriveAxisSite trackAxis;

            public TrackHeleperDummy[] helperDummies;
        }

        [System.Serializable]
        public class TrackHeleperDummy
        {
            [SerializeField] private Transform helperDummy;

            [SerializeField] private Transform forwardDummy;
            [SerializeField] private Transform backwardDummy;
            
            private Transform holder;
            private Transform forwardDummyHolder;
            private Transform backwardDummyHolder;
            
            public Transform HelperDummy => helperDummy;
            public Transform Holder => holder;
            public Transform ForwardDummy => forwardDummy;
            public Transform ForwardDummyHolder => forwardDummyHolder;
            public Transform BackwardDummy => backwardDummy;
            public Transform BackwardDummyHolder => backwardDummyHolder;

            public void Initialize()
            {
                holder = helperDummy?.GetChild(0);

                forwardDummyHolder = forwardDummy?.GetChild(0);
                backwardDummyHolder = backwardDummy?.GetChild(0);
            }
        }

        private const string TEXTURE_ALBEDO_MAP = "_BaseMap";

        [Inject] private readonly VehicleStatsBase vehicleStats;

        [SerializeField] private float idlersRotatingSpeedMultiplier = 3f;
        [SerializeField] private TrackProperties[] tracksList;

        private IEnumerable<TrackProperties> leftTracks;
        private IEnumerable<TrackProperties> rightTracks;

        public override void Initialize()
        {
            base.Initialize();

            if (tracksList != null && tracksList.Any())
            {
                leftTracks = tracksList.Where(track => track.trackAxis == DriveAxisSite.Left);
                rightTracks = tracksList.Where(track => track.trackAxis == DriveAxisSite.Right);

                foreach(var track in tracksList)//additional dummies initialization (inbewtween bones)
                {
                    if (track.helperDummies.Any())
                    {
                        foreach(var dummy in track.helperDummies)
                        {
                            dummy.Initialize();
                        }
                    }
                }
            }
        }

        protected override void FixedUpdate()
        {
            if (vehicleModelEffects != null && vehicleModelEffects.IsInsideCameraView)
            {
                base.FixedUpdate();

                TrackMovement();
            }
        }

        public override void RotateWheel(float verticalDir, Vector3 rotateAroundAxis, Transform tireTransform, UTAxlePair pair)
        {
            base.RotateWheel(verticalDir, rotateAroundAxis, tireTransform, pair);
            float rawHorizontal = inputProvider.SignedHorizontal;
            float idlerMultiplier = pair.IsIdler ? idlersRotatingSpeedMultiplier : 1f; //we want idler wheels to rotate faster than normal wheels if they are smaller

            if (inputProvider.SignedVertical == 0 && rawHorizontal != 0)//stationary tank rotating
            {
                var multiplier = 0.3f;
                pair.RotationalPartOfTire.RotateAround(tireTransform.position, rotateAroundAxis, GetTankWheelRotationInputDir(rawHorizontal, pair.Axis) * (multiplier * idlerMultiplier * 1300f) * Time.deltaTime);
            }
            else
            {
                float speed = controller.CurrentSpeed;

                if (rawHorizontal != 0f && (int)pair.Axis == rawHorizontal)//moving fwd/bwd and turning in the same time
                {
                    speed *= 0.6f; //whenever we go forward we want one side of wheels to move slower
                }
                
                pair.RotationalPartOfTire.RotateAround(tireTransform.position, rotateAroundAxis, verticalDir  * (speed * idlerMultiplier * 25f) * Time.deltaTime);
            }
        }

        public override void DummiesMovement(Transform tireTransform, UTAxlePair pair, Vector3 finalWheelPosition, float trackMovementSpeed)
        {
            base.DummiesMovement(tireTransform, pair, finalWheelPosition, trackMovementSpeed);
            var dummyPair = pair.WheelDummyPair;

            if (dummyPair.trackDummy != null)
            {
                var mainDummy = dummyPair.trackDummy;
                var desiredPos = finalWheelPosition + (pair.Wheel.Transform.up * dummyPair.dummyOffsetY);
                float localDesiredOffsetY = mainDummy.transform.InverseTransformPoint(desiredPos).y;
                mainDummy.Holder.localPosition = Vector3.Lerp(mainDummy.Holder.localPosition, new Vector3(0, localDesiredOffsetY, 0), trackMovementSpeed);
            
                if (mainDummy.UpwardDummy != null)
                {
                    float desiredUpwardsOffsetY = Mathf.Lerp(mainDummy.UpwardDummyHolder.localPosition.y, 
                        mainDummy.transform.InverseTransformPoint(desiredPos + (mainDummy.UpwardDummy.up * mainDummy.UpwardDummyOffset)).y,
                        trackMovementSpeed);

                    mainDummy.UpwardDummyHolder.localPosition =
                        new Vector3(0, desiredUpwardsOffsetY, 0);
                }
            }
        }

        private void TrackMovement()
        {
            if (tracksList != null && tracksList.Any() && !controller.IsUpsideDown && controller.CurrentSpeed != 0)
            {
                var (left, right) = GetTrackSideMultipliers(inputProvider.LastVerticalInput);

                float offset;

                if (left == right)
                {
                   offset = controller.CurrentSpeed * Time.deltaTime;
                }
                else
                { 
                    offset = controller.CurrentTurningSpeed * Time.deltaTime; 
                }

                RotateTrackTexture(leftTracks, offset, left);
                RotateTrackTexture(rightTracks, offset, right);
            }
        }
       
        private (float left, float right) GetTrackSideMultipliers(float lastSignedVertical)
        {
            if (inputProvider.AbsoluteHorizontal > 0)
            {
                if (inputProvider.AbsoluteVertical > 0)
                {
                    float leftSigned = inputProvider.SignedHorizontal > 0f ? inputProvider.AbsoluteHorizontal : inputProvider.AbsoluteHorizontal * 0.5f;
                    float rightSigned = inputProvider.SignedHorizontal > 0f ? inputProvider.AbsoluteHorizontal * 0.5f : inputProvider.AbsoluteHorizontal;

                    return (leftSigned, rightSigned);
                }

                return (inputProvider.SignedHorizontal, -inputProvider.SignedHorizontal);
            }
            else
            {
                return (lastSignedVertical, lastSignedVertical);
            }
        }

        private void RotateTrackTexture(IEnumerable<TrackProperties> trackList, float currentOffset, float sideInput)
        {
            if (trackList.Any())
            {
                foreach (var rend in trackList)
                {
                    var material = rend.trackObject.materials[0];
                    float currentTextureOffset = material.GetTextureOffset(TEXTURE_ALBEDO_MAP).y;
                    rend.trackObject.materials[0].SetTextureOffset(TEXTURE_ALBEDO_MAP, new Vector2(0f, currentTextureOffset - (currentOffset / 70f) * sideInput));

                    if (!rend.helperDummies.Any())
                    {
                        return;
                    }

                    foreach (var dummy in rend.helperDummies)
                     {
                         if (dummy.ForwardDummy != null && dummy.BackwardDummy != null)
                         {
                             float middleY = dummy.HelperDummy.InverseTransformPoint((dummy.ForwardDummyHolder.position + dummy.BackwardDummyHolder.position) * 0.5f).y;
                             dummy.Holder.localPosition = new Vector3(0f, middleY, 0f);
                         }
                     }
                }
            }
        }

        private float GetTankWheelRotationInputDir(float rawHorizontal, DriveAxisSite driveAxis)
        {
            return driveAxis == DriveAxisSite.Left ? -rawHorizontal : rawHorizontal;
        }
    }
}
