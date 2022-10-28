using Frontend.Scripts.Interfaces;
using Frontend.Scripts.Models;
using GLShared.General.Enums;
using GLShared.General.ScriptableObjects;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Progress;
using Zenject;
using System.Linq;

namespace Frontend.Scripts.Components
{
    public class UTTankController : UTVehicleController
    {
        [SerializeField] private bool airControl = true;

        private bool isBrake;
        private float inputY;

        private float currentDriveForce = 0;
        private float currentLongitudalGrip;
        private float forwardForce;
        private float turnForce;
        private Vector3 wheelVelocityLocal;

    }
}
