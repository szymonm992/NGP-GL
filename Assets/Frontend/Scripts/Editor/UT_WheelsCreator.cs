using UnityEditor;
using UnityEngine;
using System.Collections.Generic;


namespace Frontend.Scripts.Editor
{
    public class UT_WheelsCreator : EditorWindow
    {
        private GameObject wheelsParent;
        private string wheelsName;
        private bool isNumered;
        private float xOffset;

        [MenuItem("UT System/Tank helpers/Wheels creator")]
        public static void ShowWindow()
        {
            GetWindowWithRect<UT_WheelsCreator>(new Rect(0, 0, 350, 115), false, "Wheels creator");
        }

        private void OnGUI()
        {
            wheelsParent = EditorGUILayout.ObjectField("Select a parent node", wheelsParent, typeof(GameObject), true) as GameObject;
            wheelsName = EditorGUILayout.TextField("Wheels name", wheelsName);
            isNumered = EditorGUILayout.Toggle("Count numbers", isNumered);
            xOffset = EditorGUILayout.FloatField("Wheels X position", xOffset);
            EditorGUILayout.Space();

            if (GUILayout.Button("Create"))
            {
                if (AnyError())
                {
                    return;
                }
                int childCount = wheelsParent.transform.childCount;
                for (int i = 0; i < childCount; i++)
                {
                    Transform rootNode = wheelsParent.transform.root;
                    GameObject actualChild = wheelsParent.transform.GetChild(i).gameObject;

                    GameObject creatingObj = new GameObject();
                    creatingObj.transform.SetPositionAndRotation(actualChild.transform.position, rootNode.rotation);

                    creatingObj.name = wheelsName;
                    if (isNumered)
                    {
                        creatingObj.name += "(" + (i + 1) + ")";
                    }
                    creatingObj.transform.SetParent(wheelsParent.transform);
                    creatingObj.transform.localPosition = new Vector3(xOffset, creatingObj.transform.localPosition.y, creatingObj.transform.localPosition.z);

                    GameObject holder = new GameObject();
                    holder.transform.SetPositionAndRotation(creatingObj.transform.position, creatingObj.transform.rotation);

                    holder.name = "HOLDER";
                    holder.transform.SetParent(creatingObj.transform);
                    holder.transform.localPosition = Vector3.zero;

                    actualChild.transform.SetParent(holder.transform);
                    creatingObj.transform.SetSiblingIndex(i);
                }

                Debug.Log("[UT] Created " + childCount + " wheels");
            }
        }

        private bool AnyError()
        {
            if (string.IsNullOrEmpty(wheelsName))
            {
                Debug.LogError("You have to enter the name of objects first!");
                return true;
            }
            if (wheelsParent == null)
            {
                Debug.LogError("You have to select a parent node first!");
                return true;
            }
            return false;
        }
    }
}
