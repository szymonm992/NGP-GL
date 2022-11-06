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

            public TrackHeleperDummy[] helperDummies;
        }
        [System.Serializable]
        public class TrackHeleperDummy
        {
            public Transform helperDummy;
            public float raycastRange;

            [SerializeField] private float restOffset = 0.8f;
            [SerializeField] private float contactOffset = 0f;

            private Transform holder;
            
            public Transform Holder => holder;
            public float RestOffset => restOffset;
            public float ContactOffset => contactOffset;

            public void Initialize()
            {
                holder = helperDummy.GetChild(0);
            }
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

                
                foreach(var track in tracksList)//additional dummies initialization (inbewtween bones)
                {
                    if(track.helperDummies.Any())
                    {
                        foreach(var dummy in track.helperDummies)
                        {
                            dummy.Initialize();
                        }
                    }
                }
            }
        }

        public override void RotateWheels(float verticalDir, Vector3 rotateAroundAxis, Transform tireTransform, UTAxlePair pair)
        {
            base.RotateWheels(verticalDir, rotateAroundAxis, tireTransform, pair);

            float rawHorizontal = inputProvider.SignedHorizontal;
            if (inputProvider.SignedVertical == 0 && rawHorizontal != 0)//stationary tank rotating
            {
                var multiplier = 0.3f;
                pair.RotationalPartOfTire.RotateAround(tireTransform.position, rotateAroundAxis, GetTankWheelRotationInputDir(rawHorizontal, pair.Axis) * (multiplier * 1300f) * Time.deltaTime);
            }
            else
            {
                float speed = controller.CurrentSpeed;

                if (rawHorizontal != 0 && (int)pair.Axis == rawHorizontal)//moving fwd/bwd and turning in the same time
                {
                    speed /= 2f; //whenever we go forward we want one side of wheels to move slower
                }
                pair.RotationalPartOfTire.RotateAround(tireTransform.position, rotateAroundAxis, verticalDir * (speed * 25f) * Time.deltaTime);

            }
        }

        public override void TrackMovement(Transform tireTransform, UTAxlePair pair, Vector3 finalWheelPosition, float trackMovementSpeed)
        {
            base.TrackMovement(tireTransform, pair, finalWheelPosition, trackMovementSpeed);
            var dummyPair = pair.WheelDummyPair;

            if (dummyPair.trackDummy != null)
            {
                Vector3 desiredPos = finalWheelPosition + (pair.Wheel.transform.up * dummyPair.dummyOffsetY);
                Vector3 local = dummyPair.trackDummy.InverseTransformPoint(desiredPos);
                dummyPair.Holder.localPosition = Vector3.Lerp(dummyPair.Holder.localPosition, new Vector3(0, local.y, 0), trackMovementSpeed);
            }

            
            if (tracksList != null && tracksList.Any() && controller.CurrentSpeed != 0)
            {
                var leftAndRight = GetTrackSideMultipliers(inputProvider.LastVerticalInput);

                float l = leftAndRight.Item1;
                float r = leftAndRight.Item2;

                float offset;

                if (l == r)
                {
                   offset = (controller.CurrentSpeed / 5f) * Time.deltaTime;
                }
                else
                { 
                    offset = trackTurningRotationSpeed * Time.deltaTime; 
                }

                RotateTrackTexture(leftTracks, offset, l, trackMovementSpeed);
                RotateTrackTexture(rightTracks, offset, r, trackMovementSpeed);
            }
        }

        private (float, float) GetTrackSideMultipliers(float lastSignedVertical)
        {
            if(inputProvider.AbsoluteHorizontal > 0)
            {
                if (inputProvider.AbsoluteVertical > 0)
                {
                    float leftSigned = inputProvider.SignedHorizontal > 0 ? inputProvider.AbsoluteHorizontal : inputProvider.AbsoluteHorizontal / 2;
                    float rightSigned = inputProvider.SignedHorizontal > 0 ? inputProvider.AbsoluteHorizontal / 2 : inputProvider.AbsoluteHorizontal;
                    return (leftSigned, rightSigned);
                }
                return (inputProvider.SignedHorizontal, -inputProvider.SignedHorizontal);
            }
            else
            {
                return (lastSignedVertical, lastSignedVertical);
            }
        }

        private void RotateTrackTexture(IEnumerable<TrackProperties> trackList, float currentOffset, float sideInput,float movementSpeed)
        {
            if (trackList.Any())
            {
                foreach (var rend in trackList)
                {
                    var materials = rend.trackObject.materials;
                    float currentTextureOffset = materials[0].GetTextureOffset("_BaseMap").y;

                    materials[0].SetTextureOffset("_BaseMap", new Vector2(0, currentTextureOffset - (currentOffset / 70f) * sideInput));
                    rend.trackObject.materials = materials;
                    
                    if(!rend.helperDummies.Any())
                    {
                        return;
                    }

                    foreach(var dummy in rend.helperDummies)
                    {
                        var helperDummy = dummy.helperDummy;
                        Ray ray = new Ray(helperDummy.position, -helperDummy.up);
                        Vector3 desiredDummyHolderPos;
                        if (Physics.Raycast(ray, out RaycastHit hit, dummy.raycastRange))
                        {
                            desiredDummyHolderPos = new Vector3(helperDummy.position.x, hit.point.y, helperDummy.position.z) + (helperDummy.up * dummy.ContactOffset);
                        }
                        else
                        {
                            desiredDummyHolderPos = helperDummy.position - (helperDummy.up * dummy.RestOffset);
                        }
                        dummy.Holder.position = Vector3.Lerp(dummy.Holder.position, desiredDummyHolderPos, movementSpeed);
                        //Debug.DrawRay(ray.origin, ray.direction, Color.cyan);
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
