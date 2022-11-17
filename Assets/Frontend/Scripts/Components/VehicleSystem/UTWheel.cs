using System.Linq;
using GLShared.General.Utilities;
using UnityEditor;
using UnityEngine;
using Zenject;
using GLShared.General.ScriptableObjects;
using GLShared.General.Models;
using GLShared.General.Interfaces;
using GLShared.General.Enums;

namespace GLShared.General.Components
{
    public class UTWheel : UTWheelBase, IInitializable, IPhysicsWheel
    {

#if UNITY_EDITOR
        #region DEBUG
        private void OnValidate()
        {
            if (rig == null)
            {
                rig = transform.GetComponentInParent<Rigidbody>();
                localRig = GetComponent<Rigidbody>();
                localCollider = GetComponent<MeshCollider>();
            }

            AssignPrimaryParameters();
            SetIgnoredColliders();
        }


        private void OnDrawGizmos()
        {

            bool drawCurrently = (debugSettings.DrawGizmos) && (debugSettings.DrawMode == UTDebugMode.All)
                || (debugSettings.DrawMode == UTDebugMode.EditorOnly && !Application.isPlaying)
                || (debugSettings.DrawMode == UTDebugMode.PlaymodeOnly && Application.isPlaying);

            if (drawCurrently && (this.enabled) || (debugSettings.DrawOnDisable && !this.enabled))
            {
                if (rig != null)
                {
                    if (!Application.isPlaying)
                    {
                        tirePosition = GetTirePosition();
                    }

                    if (debugSettings.DrawSprings)
                    {
                        Handles.DrawDottedLine(transform.position, tirePosition, 1.1f);


                        if (upperConstraintTransform != null)
                        {
                            Handles.color = Color.white;
                            //Gizmos.color = Color.yellow;
                            Handles.DrawLine(upperConstraintTransform.position + transform.forward * 0.1f, upperConstraintTransform.position - transform.forward * 0.1f, 2f);
                            //Gizmos.DrawSphere(highestPointTransform.position, .08f);
                        }

                        if (lowerConstraintTransform != null)
                        {
                            Handles.color = Color.white;
                            //Gizmos.color = Color.yellow;
                            Handles.DrawLine(lowerConstraintTransform.position + transform.forward * 0.1f, lowerConstraintTransform.position - transform.forward * 0.1f, 2f);

                            //Gizmos.DrawSphere(lowestPointTransform.position, .08f);
                        }

                        Handles.color = Color.white;
                        //Gizmos.DrawSphere(tirePosition, .08f);
                        Handles.DrawLine(tirePosition + transform.forward * 0.05f, tirePosition - transform.forward * 0.05f, 4f);

                    }

                    if (isGrounded)
                    {
                        Gizmos.color = Color.blue;
                        Gizmos.DrawSphere(hitInfo.Point, .08f);
                    }

                    if (debugSettings.DrawWheelDirection)
                    {
                        Handles.color = isGrounded ? Color.green : Color.red;
                        Handles.DrawLine(tirePosition, tirePosition + transform.forward, 2f);
                    }



                    if (debugSettings.DrawShapeGizmo)
                    {
                        Gizmos.color = isGrounded ? Color.green : Color.red;
                        //Gizmos.DrawWireSphere(tirePosition, wheelRadius);

                        Gizmos.DrawWireMesh(localCollider.sharedMesh, tirePosition, transform.rotation, transform.lossyScale);
                    }
                }
            }
        }

        #endregion
#endif
    }
}
