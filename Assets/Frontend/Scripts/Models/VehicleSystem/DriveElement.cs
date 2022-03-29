using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Frontend.Scripts.Models.VehicleSystem
{
    [System.Serializable]
    public class DriveElement
    {
        [SerializeField] private Rigidbody wheelRigidbody;
        [SerializeField] private DriveAxisInfo axisInfo;
        [SerializeField] private bool canSteer;
        [SerializeField] private bool drive=true; //determines whether the wheel is drivable

        private Transform forceAtPos;
        private Transform stepDetector;
        private Transform visualWheel;
        private Transform meshTransform;
        private Collider collider;
        public bool Drive => drive;
        public bool CanSteer => canSteer;
        public Rigidbody WheelRigidbody => wheelRigidbody;
        public DriveAxisInfo AxisInfo => axisInfo;
        public Transform ForceAtPosition => forceAtPos;
        public Transform StepDetector => stepDetector;
        public Transform VisualWheel => visualWheel;
        public Transform MeshTransform => meshTransform;
        public Collider Collider => collider;
        public void Initialize()
        {
            this.collider = wheelRigidbody?.gameObject.GetComponent<Collider>();
            this.forceAtPos = wheelRigidbody?.transform.Find("forceAtPosition");
            this.stepDetector = wheelRigidbody?.transform.Find("stepDetector");
            this.visualWheel = wheelRigidbody?.transform.Find("visual");
            this.meshTransform = visualWheel?.GetChild(0);
        }
    }
}
