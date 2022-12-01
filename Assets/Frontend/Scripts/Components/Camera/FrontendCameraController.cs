
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

using Frontend.Scripts.Models;
using Frontend.Scripts.Interfaces;
using Frontend.Scripts.Signals;
using GLShared.General.Interfaces;

namespace Frontend.Scripts.Components
{
    public class FrontendCameraController : MonoBehaviour, IInitializable, IMouseActionsProvider
    {
        [Inject] private readonly Camera controlledCamera;
        [Inject] private readonly SignalBus signalBus;

        [Header("General Settings")]
        [SerializeField] private int reticlePixelsOffset = 75;//cursor offset from middle of screen, measured in pixels

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

        private VehicleStatsBase parameters;
        private IPlayerInputProvider inputProvider;

        [Header("Object Attachment")]
        public LayerMask targetMask;
        private Transform orbitFollowPoint;
        private Transform snipingFollowPoint;
        private GameObject currentPlayerObject;

        private float desiredOrbitDist;
        private float desiredSnipingZoom;
        private float orbitDist;
        private float snipingZoom;

        private bool isSniping = false;
        
        private Vector3 targetPosition;
        private Vector3 orbitFollowPos;
        private Vector3 snipingFollowPos;

        private Quaternion oldSnipingFollowRot;

        private bool turrentRotationLock = false, isAlive = true, blockCtrl = false;

        public float ReticlePixelsOffset => reticlePixelsOffset;
        public Vector3 CameraTargetingPosition => targetPosition;

        public void Initialize()
        {
            signalBus.Subscribe<BattleSignals.CameraSignals.OnCameraBound>(OnCameraBoundToPlayer);
        }

        private void OnCameraBoundToPlayer(BattleSignals.CameraSignals.OnCameraBound OnCameraBound)
        {
            currentPlayerObject = OnCameraBound.context.transform.gameObject;
            parameters = OnCameraBound.context.Container.Resolve<VehicleStatsBase>();
            inputProvider = OnCameraBound.inputProvider;
            FurtherAssigningLogic(OnCameraBound.startingEulerAngles);
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
                playerObject = currentPlayerObject,
                IsSniping = isSniping
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

            //handle rotation desired by player first
            HandleCameraControl();
            UpdatePosition();

            //prefpare for handling target lock
            PrepareTargetLock();

            //switching camera modes
            if (isAlive)
            {
                if (inputProvider.SnipingKey)
                {
                    SetSniping(!isSniping);
                }
            }

            // update depending on camera mode
            if (isSniping)
            { 
                UpdateSnipingCamera();
            }
            else
            { 
                UpdateOrbitCamera();
            }

            //target lock is being executed only if active camera is orbiting camera
            //or it has begun a transition between camera modes
            ApplyTargetLock();

        }

        private void FixedUpdate()
        {
            if (!currentPlayerObject)
            {
                return;
            }
            //we send camera targeting position on the server
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
        }

        // rotation, based on player inputs
        private void HandleCameraControl()
        {

            float rotY = GetUserInputs().axisX * sensitivityX;
            float rotX = GetUserInputs().axisY * sensitivityY;

            if (Input.GetKey(KeyCode.LeftControl))
            {
                rotY *= 0;
                rotX *= 0;
            }

            if (isSniping)
            {
                float zoomMult = 1 - (1 - snipingMaxSensScale) * ((snipingZoom - snipingMinZoom) / (snipingMaxZoom - snipingMinZoom));
                float finalSensScale = snipingSensScale * zoomMult;

                rotX *= finalSensScale;
                rotY *= finalSensScale;

                //cancelling previous rotation relative sniping camera
                transform.rotation = Quaternion.Inverse(snipingFollowPoint.rotation) * transform.rotation;
            }

            // TODO: rework it to use quaternion instead of filthy, feeble, misrable fucking euler angles btw
            Vector3 angles = transform.localEulerAngles;

            angles.x += rotX;
            angles.y += rotY;

            transform.localEulerAngles = angles;

            if (isSniping)
            {
                //applying again the rotation of sniping camera
                transform.rotation = snipingFollowPoint.rotation * transform.rotation;
            }


            // limiting the angle range
            float yMinRange = orbitVertMinRange;
            float yMaxRange = orbitVertMaxRange;

            if (isSniping)
            {
                yMaxRange = parameters.GunDepression;
                yMinRange = -parameters.GunElevation;
            }

            transform.rotation = LimitCameraRange(transform.rotation, yMinRange, yMaxRange);

        }

        //limits rotation of camera into set ranges
        private Quaternion LimitCameraRange(Quaternion oldRotation, float yMinRange, float yMaxRange)
        {
            Quaternion newRotation = oldRotation;

            if (isSniping)
            {
                // TODO - adding a range here
                yMaxRange = parameters.GunDepression;
                yMinRange = -parameters.GunElevation;

                //canceling previous relative rotation of sniping camera
                newRotation = Quaternion.Inverse(snipingFollowPoint.rotation) * newRotation;
            }

            Vector3 angles = newRotation.eulerAngles;

            if (angles.x > 180) angles.x -= 360;
            angles.x = Mathf.Clamp(angles.x, yMinRange, yMaxRange);

            newRotation.eulerAngles = angles;

            if (isSniping)
            {
                //applying again the relative rotation of sniping camera
                newRotation = snipingFollowPoint.rotation * newRotation;
            }

            return newRotation;
        }

        // ustawia pozycjê do predenifniowanych punktów
        // ta operacja musi byæ wykonana przed pobraniem targeta do targetlocka
        // inaczej gra dzia³a tak jakbyœ mia³ perfidnego auto-aima XD
        private void UpdatePosition()
        {
            orbitFollowPos = orbitFollowPoint.position;
            snipingFollowPos = snipingFollowPoint.position;

            if (isSniping)
            {
                transform.position = snipingFollowPos;
            }
            else
            {
                //zapobieganie przenikaniu kamery pod ziemiê w przypadku gdyby czog³ siê wyjeba³ hehe
                var dotDir = Vector3.Dot(Vector3.up, orbitFollowPoint.up);
                if (dotDir < 0)
                {
                    //orbitFollowPos.y -= dotDir * 2.0f * orbitFollowPoint.localPosition.y;
                    orbitFollowPos.y -= dotDir * orbitFollowPoint.localPosition.y;
                }
                transform.position = orbitFollowPos;
            }
        }

        private void UpdateOrbitCamera()
        {
            if (GetUserInputs().zoomValue != 0)
            {
                var newOrbitDist = desiredOrbitDist - GetUserInputs().zoomValue * 10.0f * orbitZoomStep;
                desiredOrbitDist = Mathf.Clamp(newOrbitDist, orbitMinDist, orbitMaxDist);
            }

            //target lock system handles everything so we only need to change this variable
            orbitDist = Mathf.Lerp(orbitDist, desiredOrbitDist, Time.deltaTime * orbitDistInterp * 10.0f);
            //orbitDist = desiredOrbitDist;

            // handling walls collisions
            const float camOffset = 0.25f;
            RaycastHit hit;
            //Vector3 orbitCamDirVec = controlledCamera.transform.position - transform.position;
            Quaternion orbitCamDirRotation = LimitCameraRange(GetTargetLockOrbitLookVector(true), orbitVertMinRange, orbitVertMaxRange);

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
            if (GetUserInputs().zoomValue != 0)
            {
                var newSnipingZoom = desiredSnipingZoom + GetUserInputs().zoomValue * 10.0f * snipingZoomStep;
                desiredSnipingZoom = Mathf.Clamp(newSnipingZoom, snipingMinZoom, snipingMaxZoom);
            }

            snipingZoom = Mathf.Lerp(snipingZoom, desiredSnipingZoom, Time.deltaTime * snipingZoomInterp * 10.0f);
            controlledCamera.fieldOfView = (float)(snipingFov / Math.Pow(snipingZoom, 4.0f));
            controlledCamera.transform.localPosition = Vector3.zero;

            //rotation update executed here, so target lock can handle it
            transform.rotation = snipingFollowPoint.rotation * Quaternion.Inverse(oldSnipingFollowRot) * transform.rotation;
            oldSnipingFollowRot = snipingFollowPoint.rotation;
        }

        public void SetSniping(bool snipingValue)
        {
            if (isSniping == snipingValue)
            {
                return;
            }

            isSniping = snipingValue;

            UpdatePosition();

            //fixing variables while change
            if (!snipingValue)
            {
                controlledCamera.cullingMask |= 1 << LayerMask.NameToLayer("ExcludedFromSniper");
                controlledCamera.fieldOfView = orbitFov;
                orbitDist = desiredOrbitDist;
                transform.rotation = Quaternion.identity;
            }
            else
            {
                controlledCamera.cullingMask &= ~(1 << LayerMask.NameToLayer("ExcludedFromSniper"));
                snipingZoom = snipingMinZoom;
                desiredSnipingZoom = snipingZoom;

                oldSnipingFollowRot = Quaternion.identity;
            }

            signalBus.Fire(new BattleSignals.CameraSignals.OnCameraModeChanged()
            {
                playerObject = currentPlayerObject,
                IsSniping = isSniping
            });
        }


        // Przygotowanie do wykonania funkcji ApplyTargetLock na koñcu Update'a.
        // znalezienie punktu, w który gracz celuje kamer¹. Powinno byæ wykonane 
        // po kontroli gracza ale przed operacjami, które nieumyuœlnie zmieniaj¹ cel gracza.
        private void PrepareTargetLock()
        {
            const int targetDist = 2000;

            RaycastHit hit;
            Vector3 camTransform = transform.position;
            Vector3 rayDir = transform.forward;
            if (!isSniping)
            {
                // obliczanie docelowego punktu kamery (zwi¹zany z przesuniêtym celownikiem).
                camTransform = transform.position - controlledCamera.transform.rotation * new Vector3(0, 0, orbitDist);
                rayDir = Quaternion.AngleAxis(-GetCrosshairAngle(), transform.right) * controlledCamera.transform.rotation * Vector3.forward;

                // quick fix: przesuniêcie punktu pocz¹tkowego (ignorowanie elementów za wie¿yczk¹)
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

        // Kamera przy oddalaniu i przybli¿aniu mo¿e odchodziæ od celowanego punktu, poniewa¿
        // celowany punkt nie jest na œrodku ekranu. Ta funkcja upewnia siê, ¿e dokonane
        // operacje nie powoduj¹ "odejœcia" celownika od wczeœniej wyznaczonego celu.
        // Powinna byæ wykonana wy³¹cznie na koñcu Update'a.
        private void ApplyTargetLock()
        {

            Quaternion finalRotation;
            // obliczanie k¹ta w kamerze snajpeskiej jest dziecinnie proste LOL
            if (isSniping)
            {
                Vector3 finalLookVec = (targetPosition - transform.position).normalized;
                Vector3 upVector = snipingFollowPoint.up;
                finalRotation = Quaternion.LookRotation(finalLookVec, upVector);
            }
            else
            {
                finalRotation = GetTargetLockOrbitLookVector(false);
            }
            // aplikowanie wyliczonego wektora kierunkowego
            transform.rotation = finalRotation;
        }

        // pobranie dodatkowego k¹ta celowania bazuj¹cego na offsecie pionowym kursora
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

            // matematyczny mocarz - obliczenie k¹tu pomiêdzy celem a œrodkiem systemu kamery (twierdzenie sinusów)
            float targetAngle = Mathf.Asin(Mathf.Sin(ang * Mathf.Deg2Rad) * usedOrbitDist / targetDist) * Mathf.Rad2Deg;
            // wyliczenie wektora kierunkowego - targetDir obrócony o sumê wyliczonych k¹tów
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

        private struct Inputs
        {
            public float axisX;
            public float axisY;
            public float zoomValue;
        }

        private Inputs GetUserInputs()
        {
            Inputs inputs;

            inputs.axisX = Input.GetAxis("Mouse X");
            inputs.axisY = Input.GetAxis("Mouse Y");
            inputs.zoomValue = Input.GetAxis("Zoom");
            return inputs;
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
            return isSniping;
        }

        #endregion
    }
}
