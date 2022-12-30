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

        private IPlayerInputProvider inputProvider;

        private float gunDepression;
        private float gunElevation;

        [Header("Object Attachment")]
        public LayerMask targetMask;
        private Transform orbitFollowPoint;
        private Transform snipingFollowPoint;
        public GameObject currentPlayerObject;

        private float desiredOrbitDist;
        private float desiredSnipingZoom;
        private float orbitDist;
        private float snipingZoom;

        private bool preventingBump;
        private float lastBumpTime;
        private RangedFloat bumpPreventionRange = new();
        private Quaternion previousBumpedRotation = Quaternion.identity;

        private CameraMode cameraMode = CameraMode.Orbiting;
        
        private Vector3 targetPosition;
        private Vector3 orbitFollowPos;
        private Vector3 snipingFollowPos;

        private Quaternion oldSnipingFollowRot;

        private bool turrentRotationLock = false, isAlive = true, blockCtrl = false;

        public float ReticlePixelsOffset => reticlePixelsOffset;
        public Vector3 CameraTargetingPosition => targetPosition;
        public GameObjectContext context;
        public void Initialize()
        {
            signalBus.Subscribe<BattleSignals.CameraSignals.OnCameraBound>(OnCameraBoundToPlayer);
            signalBus.Subscribe<PlayerSignals.OnPlayerInitialized>(OnPlayerInitialized);
        }

        private void OnPlayerInitialized(PlayerSignals.OnPlayerInitialized OnPlayerInitialized)
        {
            var playerProperties = OnPlayerInitialized.PlayerProperties;
            context = playerProperties.PlayerContext;

            if (playerProperties.IsLocal)
            {
                signalBus.Fire(new BattleSignals.CameraSignals.OnCameraBound()
                {
                    PlayerContext = playerProperties.PlayerContext,
                    StartingEulerAngles = playerProperties.PlayerContext.transform.eulerAngles,
                    InputProvider = OnPlayerInitialized.InputProvider,
                    GunDepression = OnPlayerInitialized.GunDepression,
                    GunElevation = OnPlayerInitialized.GunElevation,
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
                float zoomMult = 1 - (1 - snipingMaxSensScale) * ((snipingZoom - snipingMinZoom) / (snipingMaxZoom - snipingMinZoom));
                float finalSensScale = snipingSensScale * zoomMult;

                rotation *= finalSensScale;

                // cancelling previous rotation relative to the sniping camera
                // this is done to apply the player-defined camera rotation locally.
                transform.rotation = Quaternion.Inverse(snipingFollowPoint.rotation) * transform.rotation;
            }

            transform.rotation *= Quaternion.Euler(rotation);

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
                    Mathf.Clamp(Mathf.DeltaAngle(0, newRotation.eulerAngles.x), limits.Min, limits.Max),
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
                    Mathf.Clamp(Mathf.DeltaAngle(0, newRotation.eulerAngles.x), limits.Min, limits.Max),
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
                if (dotDir < 0)
                {
                    orbitFollowPos.y -= dotDir * orbitFollowPoint.localPosition.y;
                }
                transform.position = orbitFollowPos;
            }
        }

        private void UpdateOrbitCamera()
        {
            var zoomValue = GetZoomValue();
            if (zoomValue != 0)
            {
                var newOrbitDist = desiredOrbitDist - zoomValue * 10.0f * orbitZoomStep;
                desiredOrbitDist = Mathf.Clamp(newOrbitDist, orbitMinDist, orbitMaxDist);
            }

            // target lock system handles everything so we only need to change this variable
            orbitDist = Mathf.Lerp(orbitDist, desiredOrbitDist, Time.deltaTime * orbitDistInterp * 10.0f);

            // handling walls collisions
            const float camOffset = 0.25f;
            RaycastHit hit;
            Quaternion orbitCamDirRotation = LimitCameraRange(GetTargetLockOrbitLookVector(true));

            Vector3 orbitCamDirVec = (orbitCamDirRotation * Vector3.forward).normalized * desiredOrbitDist * -1f;

            Ray r = new Ray(transform.position, orbitCamDirVec);
            if (Physics.Raycast(r, out hit, orbitCamDirVec.magnitude + 0.1f, targetMask))
            {
                orbitDist = Mathf.Min(hit.distance - camOffset, orbitDist);
                Debug.DrawRay(transform.position, orbitCamDirVec.normalized * hit.distance, Color.blue);
            }

            // setting the local position of camera
            controlledCamera.transform.localPosition = new Vector3(0, 0, -orbitDist);
        }

        private void UpdateSnipingCamera()
        {
            var zoomValue = GetZoomValue();
            if (zoomValue != 0)
            {
                var newSnipingZoom = desiredSnipingZoom + zoomValue * 10.0f * snipingZoomStep;
                desiredSnipingZoom = Mathf.Clamp(newSnipingZoom, snipingMinZoom, snipingMaxZoom);
            }

            snipingZoom = Mathf.Lerp(snipingZoom, desiredSnipingZoom, Time.deltaTime * snipingZoomInterp * 10.0f);
            controlledCamera.fieldOfView = (float)(snipingFov / Math.Pow(snipingZoom, 4.0f));
            controlledCamera.transform.localPosition = Vector3.zero;

            //rotation update executed here, so target lock can handle it
            transform.rotation = snipingFollowPoint.rotation * Quaternion.Inverse(oldSnipingFollowRot) * transform.rotation;
            oldSnipingFollowRot = snipingFollowPoint.rotation;
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
                transform.rotation = Quaternion.identity;
            }
            else if(newMode == CameraMode.Sniping)
            {
                controlledCamera.cullingMask &= ~(1 << LayerMask.NameToLayer("ExcludedFromSniper"));
                snipingZoom = snipingMinZoom;
                desiredSnipingZoom = snipingZoom;
                oldSnipingFollowRot = Quaternion.identity;
            }

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
            RangedFloat baseLimits = GetSnipingModeLimits();

            float currentAngle = (Quaternion.Inverse(snipingFollowPoint.rotation) * previousBumpedRotation).eulerAngles.x;

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
                    Debug.Log(lastBumpTime - Time.time);
                }

                var returnScale = Mathf.Pow(
                    Mathf.Max(0, Time.time - lastBumpTime - bumpPreventionDelay) * bumpPreventionIncrease,
                    bumpPreventionExponent
                ) * Time.deltaTime;

                bumpPreventionRange.Min = Mathf.Lerp(bumpPreventionRange.Min, baseLimits.Min, returnScale);
                bumpPreventionRange.Max = Mathf.Lerp(bumpPreventionRange.Max, baseLimits.Max, returnScale);

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
            RangedFloat baseLimits = GetSnipingModeLimits();
            
            float currentAngle = (Quaternion.Inverse(snipingFollowPoint.rotation) * transform.rotation).eulerAngles.x;

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

            previousBumpedRotation = transform.rotation;
        }

        // finding the point player is aiming towards.
        // should be done after player control but before operations
        // which can manipulate camera mode or follow point
        private void PrepareTargetLock()
        {
            const int targetDist = 2000;

            RaycastHit hit;
            Vector3 camTransform = transform.position;
            Vector3 rayDir = transform.forward;
            if (IsInOrbitingMode())
            {
                // calculate desired camera point (according to moved crosshair).
                camTransform = transform.position - controlledCamera.transform.rotation * new Vector3(0, 0, orbitDist);
                rayDir = Quaternion.AngleAxis(-GetCrosshairAngle(), transform.right) * controlledCamera.transform.rotation * Vector3.forward;

                // quick fix: move starting point (ignore objects behind the turret)
                camTransform += rayDir * orbitDist;
            }

            Ray r = new Ray(camTransform, rayDir * targetDist);
            if (!Physics.Raycast(r, out hit, targetDist, targetMask))
            {
                targetPosition = r.origin + r.direction * targetDist;
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
                finalRotation = GetTargetLockOrbitLookVector(false);
            }
            
            transform.rotation = finalRotation;
        }

        // returns aiming angle based of how much crosshair is offset from the middle of the screen
        private float GetCrosshairAngle()
        {
            return Mathf.Atan(reticlePixelsOffset * 2f * Mathf.Tan(controlledCamera.fieldOfView * 0.5f * Mathf.Deg2Rad) / Screen.height) * Mathf.Rad2Deg;
        }


        private Quaternion GetTargetLockOrbitLookVector(bool useDesiredDist)
        {
            Vector3 targetDir = targetPosition - transform.position;
            float targetDist = targetDir.magnitude;
            targetDir = targetDir.normalized;

            float usedOrbitDist = useDesiredDist ? desiredOrbitDist : orbitDist;

            float ang = GetCrosshairAngle();

            // calculating the angle between the target and the middle of camera system (sine theorem)
            float targetAngle = Mathf.Asin(Mathf.Sin(ang * Mathf.Deg2Rad) * usedOrbitDist / targetDist) * Mathf.Rad2Deg;
            // calculating direction vector - targetDir obrócony o sumê wyliczonych k¹tów
            Vector3 lookVector = Quaternion.AngleAxis(ang + targetAngle, Vector3.Cross(Vector3.up, targetDir)) * targetDir;

            return Quaternion.LookRotation(lookVector, Vector3.up);
        }




        #region HELPERS&STRUCTS
        private void FindCameraTargetObjects()
        {
            var all_objects_inside = currentPlayerObject.GetComponentsInChildren<Transform>();
            foreach (var child in all_objects_inside)
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
