using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

using Frontend.Scripts.Models;
using Frontend.Scripts.Interfaces;
using Frontend.Scripts.Signals;
using GLShared.General.Interfaces;
using GLShared.General.Signals;
using GLShared.General.Models;
using Frontend.Scripts.Enums;

namespace Frontend.Scripts.Components
{
    public class FrontendCameraController : MonoBehaviour, IInitializable, IMouseActionsProvider
    {
        [Inject] private readonly Camera controlledCamera;
        [Inject] private readonly SignalBus signalBus;
        [Inject(Optional = true)] private readonly FrontendSyncManager frontendSyncManager;

        [Header("General Settings")]
        //cursor offset from middle of screen, measured in pixels
        [SerializeField] private int reticlePixelsOffset = 75;

        [SerializeField] private float sensitivityX = 1f;
        [SerializeField] private float sensitivityY = 1f;

        [Header("Orbit Camera")]
        [SerializeField] private float orbitMinDist = 2f;
        [SerializeField] private float orbitMaxDist = 17f;
        [SerializeField] private float orbitVertMinRange = -30f;
        [SerializeField] private float orbitVertMaxRange = 60f;
        [SerializeField] private float orbitFov = 60f;
        [SerializeField] private float orbitDistInterp = 1f;
        [SerializeField] private float orbitZoomStep = 3f;
        [SerializeField] private float orbitCameraColliderSize = 0.5f;

        [Header("Sniping Camera")]
        [SerializeField] private float snipingFov = 60f;
        [SerializeField] private float snipingMaxZoom = 1.7f;
        [SerializeField] private float snipingMinZoom = 1.1f;
        [SerializeField] private float snipingZoomInterp = 1f;
        [SerializeField] private float snipingZoomStep = 0.2f;
        [SerializeField] private float snipingSensScale = 0.2f;
        [SerializeField] private float snipingMaxSensScale = 0.15f;
        [SerializeField] private float bumpPreventionDelay = 1.5f;
        [SerializeField] private float bumpPreventionIncrease = 1.0f;
        [SerializeField] private float bumpPreventionExponent = 1.0f;
        [SerializeField] private float bumpPreventionRecoverFactor = 0.1f;
        [SerializeField] private float bumpPreventionMinRange = -45f;
        [SerializeField] private float bumpPreventionMaxRange = 45f;

        private IPlayerInputProvider inputProvider;

        private float gunDepression;
        private float gunElevation;

        [Header("Object Attachment")]
        public LayerMask targetMask;
        private Transform orbitFollowPoint;
        private Transform snipingFollowPoint;
        private GameObject currentPlayerObject;

        private float desiredOrbitDist;
        private float desiredSnipingZoom;
        private float smoothOrbitDist;
        private float orbitDist;
        private float snipingZoom;

        private bool preventingBump;
        private bool pushedFromWallThisFrame;
        private float lastBumpTime;
        private RangedFloat bumpPreventionRange = new();

        private CameraMode cameraMode = CameraMode.Orbiting;
        private CameraMode lastCameraMode = CameraMode.Orbiting;
        
        private Vector3 targetPosition;
        private Vector3 targetHorizontalDirection;
        private Vector3 orbitFollowPos;
        private Vector3 snipingFollowPos;

        private Quaternion oldSnipingFollowRot;

        private bool turrentRotationLock = false, isAlive = true, blockCtrl = false;
        public float ReticlePixelsOffset => reticlePixelsOffset;
        public Vector3 CameraTargetingPosition => targetPosition;

        public void Initialize()
        {
            signalBus.Subscribe<BattleSignals.CameraSignals.OnCameraBound>(OnCameraBoundToPlayer);
            signalBus.Subscribe<PlayerSignals.OnPlayerInitialized>(OnPlayerInitialized);
        }

        private void OnPlayerInitialized(PlayerSignals.OnPlayerInitialized OnPlayerInitialized)
        {
            var playerProperties = OnPlayerInitialized.PlayerProperties;

            if (playerProperties.IsLocal)
            {
                signalBus.Fire(new BattleSignals.CameraSignals.OnCameraBound()
                {
                    PlayerContext = playerProperties.PlayerContext,
                    StartingEulerAngles = playerProperties.PlayerContext.transform.eulerAngles,
                    InputProvider = OnPlayerInitialized.InputProvider,
                    GunDepression = OnPlayerInitialized.VehicleStats.GunDepression,
                    GunElevation = OnPlayerInitialized.VehicleStats.GunElevation,
                });
            }
        }

        private void OnCameraBoundToPlayer(BattleSignals.CameraSignals.OnCameraBound OnCameraBound)
        {
            currentPlayerObject = OnCameraBound.PlayerContext.transform.gameObject;

            gunDepression = OnCameraBound.GunDepression;
            gunElevation = OnCameraBound.GunElevation;
            inputProvider = OnCameraBound.InputProvider;

            FurtherAssigningLogic(OnCameraBound.StartingEulerAngles);
        }

        private void FurtherAssigningLogic(Vector3 startingEA)
        {
            FindCameraTargetObjects();

            orbitDist = (orbitMinDist + orbitMaxDist) * 0.5f;
            snipingZoom = snipingMinZoom;

            desiredOrbitDist = orbitDist;
            smoothOrbitDist = orbitDist;
            desiredSnipingZoom = snipingZoom;

            oldSnipingFollowRot = Quaternion.identity;
            transform.root.rotation = Quaternion.Euler(startingEA);

            signalBus.Fire(new BattleSignals.CameraSignals.OnCameraModeChanged()
            {
                PlayerObject = currentPlayerObject,
                Mode = cameraMode
            });
        }

        private void OnDestroy()
        {
            signalBus.Unsubscribe<BattleSignals.CameraSignals.OnCameraBound>(OnCameraBoundToPlayer);
        }

        private void LateUpdate()
        {
            var currentCameraMode = cameraMode;

            if (!currentPlayerObject || blockCtrl)
            {
                return;
            }

            // handle player-desired rotation first
            PreUpdateBumpPrevention();
            HandlePlayerCameraControl();
            PostUpdateBumpPrevention();

            UpdatePosition();

            // prepare for target lock handling
            PrepareTargetLock();

            // switching camera modes
            if (isAlive)
            {
                if (inputProvider.SnipingKey)
                {
                    ToggleSniping();
                }
            }

            // update depending on current camera mode
            switch (cameraMode)
            {
                case CameraMode.Orbiting:
                    UpdateOrbitCamera();
                    break;
                case CameraMode.Sniping:
                    UpdateSnipingCamera();
                    break;
            }

            // target lock is being executed only if camera is currently in orbiting mode
            // or if it has begun a transition between modes
            ApplyTargetLock();

            // After target lock, the camera can rotate back into the ground.
            // To avoid recursive logic, bump camera along target vector
            PushCameraFromWallAlongTargetLine();

            lastCameraMode = currentCameraMode;

            Debug.DrawRay(controlledCamera.transform.position, targetPosition - controlledCamera.transform.position, Color.red);
        }

        private void FixedUpdate()
        {
            if (!currentPlayerObject)
            {
                return;
            }
            // we send camera targeting position on the server
            turrentRotationLock = Input.GetMouseButton(1);


            if (Input.GetKey(KeyCode.LeftControl) || blockCtrl)
            {
                Cursor.lockState = CursorLockMode.None;
                turrentRotationLock = true;
                Cursor.visible = true;
            }
            else
            {
                turrentRotationLock = false;
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }

            SendCameraTarget();
        }

        private void SendCameraTarget()
        {
            if (frontendSyncManager && !turrentRotationLock && !blockCtrl)
            {
                frontendSyncManager.LocalPlayerEntity.Input.UpdateCameraTarget(targetPosition);
            }
        }


        private void HandlePlayerCameraControl()
        {
            Vector2 rotation = new(
                GetMouseAxes().y * sensitivityY,
                GetMouseAxes().x * sensitivityX
            );

            if (Input.GetKey(KeyCode.LeftControl))
            {
                rotation = Vector2.zero;
            }

            if (IsInSnipingMode())
            {
                float zoomMult = 1f - (1f - snipingMaxSensScale) * ((snipingZoom - snipingMinZoom) / (snipingMaxZoom - snipingMinZoom));
                float finalSensScale = snipingSensScale * zoomMult;

                rotation *= finalSensScale;

                // cancelling previous rotation relative to the sniping camera
                // this is done to apply the player-defined camera rotation locally.
                transform.rotation = Quaternion.Inverse(snipingFollowPoint.rotation) * transform.rotation;
            }

            transform.rotation = Quaternion.Euler(transform.eulerAngles + (Vector3)rotation);

            if (IsInSnipingMode())
            {
                // reapplying the sniping camera rotation
                transform.rotation = snipingFollowPoint.rotation * transform.rotation;
            }

            transform.rotation = LimitCameraRange(transform.rotation);
        }


        private Quaternion LimitCameraRange(Quaternion rotation, CameraMode mode = CameraMode.None)
        {
            Quaternion newRotation = rotation;
            if (mode == CameraMode.None) mode = cameraMode;

            if (mode == CameraMode.Sniping)
            {
                RangedFloat limits = bumpPreventionRange;
                // canceling previous relative rotation of sniping camera
                newRotation = Quaternion.Inverse(snipingFollowPoint.rotation) * newRotation;

                newRotation = Quaternion.Euler(
                    Mathf.Clamp(Mathf.DeltaAngle(0f, newRotation.eulerAngles.x), limits.Min, limits.Max),
                    newRotation.eulerAngles.y,
                    newRotation.eulerAngles.z
                );

                // applying the relative rotation back
                newRotation = snipingFollowPoint.rotation * newRotation;
            }

            else if (mode == CameraMode.Orbiting)
            {
                RangedFloat limits = new(orbitVertMinRange, orbitVertMaxRange);
                newRotation = Quaternion.Euler(
                    Mathf.Clamp(Mathf.DeltaAngle(0f, newRotation.eulerAngles.x), limits.Min, limits.Max),
                    newRotation.eulerAngles.y,
                    newRotation.eulerAngles.z
                );
            }

            return newRotation;
        }

        // sets position of predefined follow points and the camera
        // this has to be executed before fetching target for target-lock
        // otherwise the game just works like auto-aim
        private void UpdatePosition()
        {
            orbitFollowPos = orbitFollowPoint.position;
            snipingFollowPos = snipingFollowPoint.position;

            if (IsInSnipingMode())
            {
                transform.position = snipingFollowPos;
            }
            else
            {
                // preventing the camera from clipping through the ground
                var dotDir = Vector3.Dot(Vector3.up, orbitFollowPoint.up);
                if (dotDir < 0f)
                {
                    orbitFollowPos.y -= dotDir * orbitFollowPoint.localPosition.y;
                }
                transform.position = orbitFollowPos;
            }
        }

        private void UpdateSnipingCamera()
        {
            var zoomValue = GetZoomValue();
            if (zoomValue != 0f)
            {
                var newSnipingZoom = desiredSnipingZoom + zoomValue * 10.0f * snipingZoomStep;
                desiredSnipingZoom = Mathf.Clamp(newSnipingZoom, snipingMinZoom, snipingMaxZoom);
                signalBus.Fire(new BattleSignals.CameraSignals.OnZoomChanged()
                {
                    CurrentZoom = desiredSnipingZoom,
                });
            }

            snipingZoom = Mathf.Lerp(snipingZoom, desiredSnipingZoom, Time.deltaTime * snipingZoomInterp * 10.0f);
            controlledCamera.fieldOfView = (float)(snipingFov / Math.Pow(snipingZoom, 4.0f));
            controlledCamera.transform.localPosition = Vector3.zero;

            //rotation update executed here, so target lock can handle it
            transform.rotation = snipingFollowPoint.rotation * Quaternion.Inverse(oldSnipingFollowRot) * transform.rotation;
            oldSnipingFollowRot = snipingFollowPoint.rotation;
        }

        private void UpdateOrbitCamera()
        {
            pushedFromWallThisFrame = false;

            var zoomValue = GetZoomValue();
            if (zoomValue != 0f)
            {
                var newOrbitDist = desiredOrbitDist - zoomValue * 10.0f * orbitZoomStep;
                desiredOrbitDist = Mathf.Clamp(newOrbitDist, orbitMinDist, orbitMaxDist);
            }

            // target lock system handles everything so we only need to change this variable
            orbitDist = desiredOrbitDist;

            // pushing the camera away from the wall
            PushCameraFromWallAlongOrbitLine();

            // Doesn't actually push the camera from the ceiling, but rather
            // limits the orbiting distance so it wouldn't break in tunnels.
            HandleDistanceWhenBelowCeiling();

            // setting the local position of camera to lerped distance
            var lerpFactor = (cameraMode != lastCameraMode) ? 1.0f : Time.deltaTime * orbitDistInterp * 10.0f;
            smoothOrbitDist = Mathf.Lerp(smoothOrbitDist, orbitDist, lerpFactor);
            controlledCamera.transform.localPosition = new(0f, 0f, -smoothOrbitDist);
        }

        private void PushCameraFromWallAlongOrbitLine()
        {
            if (!IsInOrbitingMode())
            {
                return;
            }

            var orbitCamDirRotation = LimitCameraRange(GetTargetLockOrbitLookVector(false));
            var orbitCamDirVec = (orbitCamDirRotation * Vector3.forward).normalized;

            orbitCamDirVec *= orbitDist * -1f;

            var ray = new Ray(transform.position, orbitCamDirVec);
            if (Physics.SphereCast(ray, orbitCameraColliderSize * 0.5f, out var hit, orbitCamDirVec.magnitude + (orbitCameraColliderSize * 0.5f), targetMask))
            {
                orbitDist = Mathf.Min(hit.distance - orbitCameraColliderSize, orbitDist);
                pushedFromWallThisFrame = true;
            }
        }

        private void HandleDistanceWhenBelowCeiling()
        {
            if (!IsInOrbitingMode())
            {
                return;
            }

            var ray = new Ray(transform.position, Vector3.up);
            var cosTargetAngle = Mathf.Cos((90.0f - GetCrosshairAngle()) * Mathf.Deg2Rad);
            var rayDistance = orbitDist * cosTargetAngle;
            if (Physics.SphereCast(ray, orbitCameraColliderSize * 0.5f, out var hit, rayDistance + (orbitCameraColliderSize * 0.5f), targetMask))
            {
                orbitDist = Mathf.Min((hit.distance - orbitCameraColliderSize) / cosTargetAngle, orbitDist);
                pushedFromWallThisFrame = true;
            }
        }

        private void PushCameraFromWallAlongTargetLine()
        {
            if (!IsInOrbitingMode() || !pushedFromWallThisFrame)
            {
                return;
            }

            var targetCameraDelta = controlledCamera.transform.position - targetPosition;
            var targetOriginDelta = transform.position - targetPosition;

            var targetOriginProjection = Vector3.Dot(targetCameraDelta.normalized, targetOriginDelta);
            var rayStartPoint = targetPosition + targetCameraDelta.normalized * targetOriginProjection;
            var rayDirection = targetCameraDelta.normalized * (targetCameraDelta.magnitude - targetOriginProjection);

            var ray = new Ray(rayStartPoint, rayDirection);

            if (Physics.SphereCast(ray, orbitCameraColliderSize * 0.5f, out var hit, rayDirection.magnitude, targetMask))
            {
                var offset = rayDirection.normalized * (hit.distance - rayDirection.magnitude);
                controlledCamera.transform.position += offset;
            }
        }

        public void SetCameraMode(CameraMode newMode)
        {
            if (cameraMode == newMode)
            {
                return;
            }

            cameraMode = newMode;

            UpdatePosition();

            // fixing variables after changing the mode
            if (newMode == CameraMode.Orbiting)
            {
                controlledCamera.cullingMask |= 1 << LayerMask.NameToLayer("ExcludedFromSniper");
                controlledCamera.fieldOfView = orbitFov;
                orbitDist = desiredOrbitDist;
                smoothOrbitDist = desiredOrbitDist;
                transform.rotation = Quaternion.identity;
            }
            else if(newMode == CameraMode.Sniping)
            {
                controlledCamera.cullingMask &= ~(1 << LayerMask.NameToLayer("ExcludedFromSniper"));
                snipingZoom = snipingMinZoom;
                desiredSnipingZoom = snipingZoom;
                oldSnipingFollowRot = Quaternion.identity;
                bumpPreventionRange = GetSnipingModeLimits();
                preventingBump = false;
            }

            transform.rotation = LimitCameraRange(transform.rotation, newMode);

            signalBus.Fire(new BattleSignals.CameraSignals.OnCameraModeChanged()
            {
                PlayerObject = currentPlayerObject,
                Mode = cameraMode
            });
        }

        public void ToggleSniping()
        {
            SetCameraMode(cameraMode == CameraMode.Sniping ? CameraMode.Orbiting : CameraMode.Sniping);
            
        }

        private void PreUpdateBumpPrevention()
        {
            if (!IsInSnipingMode() || lastCameraMode != CameraMode.Sniping)
            {
                return;
            }

            RangedFloat baseLimits = GetSnipingModeLimits();

            float currentAngle = (Quaternion.Inverse(snipingFollowPoint.rotation) * transform.rotation).eulerAngles.x;
            currentAngle = Mathf.DeltaAngle(0f, currentAngle);

            if (!preventingBump)
            {
                bumpPreventionRange.Min = baseLimits.Min;
                bumpPreventionRange.Max = baseLimits.Max;
                if (!baseLimits.InRange(currentAngle))
                {
                    preventingBump = true;
                    lastBumpTime = Time.time;
                }
            }
            
            if(preventingBump)
            {
                var factor = 0.0f;
                if (currentAngle > bumpPreventionRange.Max)
                {
                    factor = (currentAngle - bumpPreventionRange.Max)  * bumpPreventionRecoverFactor;
                    bumpPreventionRange.Max = currentAngle;
                }
                if(currentAngle < bumpPreventionRange.Min)
                {
                    factor = (bumpPreventionRange.Min - currentAngle) * bumpPreventionRecoverFactor;
                    bumpPreventionRange.Min = currentAngle;
                }

                if(factor != 0.0f)
                {
                    lastBumpTime = Mathf.Min(lastBumpTime + factor, Time.time);
                }

                var returnScale = Mathf.Pow(
                    Mathf.Max(0f, Time.time - lastBumpTime - bumpPreventionDelay) * bumpPreventionIncrease,
                    bumpPreventionExponent
                ) * Time.deltaTime;

                bumpPreventionRange.Min = Mathf.Lerp(bumpPreventionRange.Min, baseLimits.Min, returnScale);
                bumpPreventionRange.Max = Mathf.Lerp(bumpPreventionRange.Max, baseLimits.Max, returnScale);

                // clamp range to minimum and maximum
                bumpPreventionRange.Min = Mathf.Max(bumpPreventionRange.Min, bumpPreventionMinRange);
                bumpPreventionRange.Max = Mathf.Min(bumpPreventionRange.Max, bumpPreventionMaxRange);

                if (baseLimits.InRange(currentAngle))
                {
                    preventingBump = false;
                    bumpPreventionRange.Min = baseLimits.Min;
                    bumpPreventionRange.Max = baseLimits.Max;
                }
            }
        }

        // executed after player control has been processed,
        // so it can be used for bump prevention blockage
        private void PostUpdateBumpPrevention()
        {
            if (!IsInSnipingMode())
            {
                return;
            }

            RangedFloat baseLimits = GetSnipingModeLimits();
            
            float currentAngle = (Quaternion.Inverse(snipingFollowPoint.rotation) * transform.rotation).eulerAngles.x;
            currentAngle = Mathf.DeltaAngle(0f, currentAngle);

            if (bumpPreventionRange.InRange(currentAngle))
            {
                bumpPreventionRange.Min = Mathf.Max(Mathf.Min(currentAngle, baseLimits.Min), bumpPreventionRange.Min);
                bumpPreventionRange.Max = Mathf.Min(Mathf.Max(currentAngle, baseLimits.Max), bumpPreventionRange.Max);
            }
            if (baseLimits.InRange(currentAngle))
            {
                preventingBump = false;
                bumpPreventionRange.Min = baseLimits.Min;
                bumpPreventionRange.Max = baseLimits.Max;
            }
        }

        // finding the point player is aiming towards.
        // should be done after player control but before operations
        // which can manipulate camera mode or follow point
        private void PrepareTargetLock()
        {
            const int targetDist = 2000;

            Vector3 camTransform = transform.position;
            Vector3 rayDir = transform.forward;
            if (IsInOrbitingMode())
            {
                // calculate desired camera point (according to moved crosshair).
                camTransform = transform.position - controlledCamera.transform.rotation * new Vector3(0, 0, smoothOrbitDist);

                rayDir = Quaternion.AngleAxis(-GetCrosshairAngle(), transform.right) * controlledCamera.transform.rotation * Vector3.forward;

                // quick fix: move starting point (ignore objects behind the turret)
                camTransform += rayDir * smoothOrbitDist;
            }

            targetHorizontalDirection = Vector3.Cross(Vector3.Cross(Vector3.up, rayDir), Vector3.up);

            var ray = new Ray(camTransform, rayDir * targetDist);
            if (!Physics.Raycast(ray, out var hit, targetDist, targetMask))
            {
                targetPosition = ray.origin + ray.direction * targetDist;
            }
            else
            {
                targetPosition = hit.point;
            }
        }
        
        // the camera can deviate from target point when zooming in and out, becasue
        // aimed point is not in the middle of the screen. this function makes sure
        // no operations are causing this deviation from previously chosen target.
        // it should be executed after all of the camera operations has been done.
        private void ApplyTargetLock()
        {
            Quaternion finalRotation = transform.rotation;
            
            if (IsInSnipingMode())
            {
                Vector3 finalLookVec = (targetPosition - transform.position).normalized;
                Vector3 upVector = snipingFollowPoint.up;
                finalRotation = Quaternion.LookRotation(finalLookVec, upVector);
            }
            else if(IsInOrbitingMode())
            {
                finalRotation = GetTargetLockOrbitLookVector(true);
            }
            
            transform.rotation = finalRotation;
        }

        // returns aiming angle based of how much crosshair is offset from the middle of the screen
        private float GetCrosshairAngle()
        {
            return Mathf.Atan(reticlePixelsOffset * 2f * Mathf.Tan(controlledCamera.fieldOfView * 0.5f * Mathf.Deg2Rad) / Screen.height) * Mathf.Rad2Deg;
        }


        private Quaternion GetTargetLockOrbitLookVector(bool useRealOrbitDist)
        {
            var usedOrbitDist = useRealOrbitDist ? smoothOrbitDist : orbitDist;
            var targetDelta = targetPosition - transform.position;
            var crosshairAngle = GetCrosshairAngle();

            // Using laws of cosines to determine the angle of camera which would aim at our target.
            var cosCrosshairAngle = Mathf.Cos(crosshairAngle * Mathf.Deg2Rad);
            var delta = Mathf.Pow(usedOrbitDist * cosCrosshairAngle, 2.0f) - Mathf.Pow(usedOrbitDist, 2.0f) + Mathf.Pow(targetDelta.magnitude, 2.0f);

            // in some cases, it's impossible to form a triangle. revert to default rotation
            if(delta < 0.0f)
            {
                return transform.rotation;
            }

            var targetToCameraDist = -Mathf.Sqrt(delta) - usedOrbitDist * cosCrosshairAngle;

            var cosTargetAngle = (Mathf.Pow(targetDelta.magnitude, 2.0f) + Mathf.Pow(usedOrbitDist, 2.0f) - Mathf.Pow(targetToCameraDist, 2.0f)) / (2.0f * usedOrbitDist * targetDelta.magnitude);
            var targetAngleDifference = 180.0f - Mathf.Acos(cosTargetAngle) * Mathf.Rad2Deg;

            var finalDirection = Quaternion.AngleAxis(-targetAngleDifference, Vector3.Cross(targetHorizontalDirection, Vector3.up)) * targetDelta.normalized;
            return Quaternion.LookRotation(finalDirection, Vector3.up);
        }



        #region HELPERS&STRUCTS
        private void FindCameraTargetObjects()
        {
            var objectsInParent = currentPlayerObject.GetComponentsInChildren<Transform>();
            foreach (var child in objectsInParent)
            {
                if (child.name == "CAMERA_CENTER")
                {
                    orbitFollowPoint = child;
                }
                else if (child.name == "SNIPER_CAM_PIVOT")
                {
                    snipingFollowPoint = child;
                }
            }
            if (!currentPlayerObject)
            {
                Debug.LogError("Player object not found");
            }
        }

        private Vector2 GetMouseAxes()
        {
            return new(
                Input.GetAxis("Mouse X"),
                Input.GetAxis("Mouse Y")
            );
        }

        private float GetZoomValue()
        {
            return Input.GetAxis("Zoom");
        }

        private RangedFloat GetSnipingModeLimits()
        {
            return new(-gunElevation, gunDepression);
        }


        public void SetBlockCtrl(bool val)
        {
            this.blockCtrl = val;
        }
        public Vector3 GetTargetedPosition()
        {
            return targetPosition;
        }

        public float GetSnipingZoom()
        {
            return controlledCamera.fieldOfView;
        }

        public bool IsInSnipingMode()
        {
            return cameraMode == CameraMode.Sniping;
        }

        public bool IsInOrbitingMode()
        {
            return cameraMode == CameraMode.Orbiting;
        }

        #endregion
    }
}
