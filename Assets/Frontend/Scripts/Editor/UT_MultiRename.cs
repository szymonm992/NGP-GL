using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Frontend.Scripts.Editor
{
    public class UT_MultiRename : EditorWindow
    {
        private GameObject objectsParent;
        private string objectsNamePattern;
        private bool numeringObjects;
        private Vector2Int childRange;

        [MenuItem("UT System/Addons/Multiple name changer")]
        public static void ShowWindow()
        {
            GetWindowWithRect<UT_MultiRename>(new Rect(0, 0, 350, 155), false, "Multiple name changer");
        }

        private void OnGUI()
        {
            objectsParent = EditorGUILayout.ObjectField("Select a parent node", objectsParent, typeof(GameObject), true) as GameObject;
            objectsNamePattern = EditorGUILayout.TextField("Objects name", objectsNamePattern);
            numeringObjects = EditorGUILayout.Toggle("Count numbers", numeringObjects);
            childRange = EditorGUILayout.Vector2IntField("Children range", childRange);
            EditorGUILayout.LabelField("X - minimum | Y - maximum");
            EditorGUILayout.Space();

            if (GUILayout.Button("Create"))
            {
                if (IsFormInvalid())
                {
                    return;
                }

                int childCount = objectsParent.transform.childCount;

                if (childRange[0] > 0 && childRange[1] > 0)
                {
                    if (childRange[1] > childCount)
                    {
                        Debug.LogError("Incorrect maximum range!");
                        return;
                    }
                    else if (childRange[0] > childRange[1])
                    {
                        Debug.LogError("Incorrect minimal range!");
                    }
                    else
                    {
                        int howmany = (childRange[1] + 1) - childRange[0];
                        for (int i = (childRange[0] - 1); i <= (childRange[1] - 1); i++)
                        {
                            Execute(i, howmany);
                        }
                        Debug.Log("[UT automating systems] Renamed successfully " + howmany + " objects");
                    }
                }
                else
                {
                    for (int i = 0; i < childCount; i++)
                    {
                        Execute(i, childCount);
                    }
                    Debug.Log("[UT automating systems] Renamed successfully " + childCount + " objects");
                }
            }
        }

        private void Execute(int i, int count)
        {
            GameObject actual_child = objectsParent.transform.GetChild(i).gameObject;

            actual_child.name = objectsNamePattern;
            if (numeringObjects) actual_child.name += "(" + (i + 1) + ")";
        }

        private bool IsFormInvalid()
        {
            if (string.IsNullOrEmpty(objectsNamePattern))
            {
                Debug.LogError("You have to enter the name of objects first!");
                return true;
            }
            if (objectsParent == null)
            {
                Debug.LogError("You have to select a parent node first!");
                return true;
            }
            if (childRange[0] < 0 || childRange[1] < 0)
            {
                Debug.LogError("Incorrect range values!");
                return true;
            }
            return false;
        }
    }
}
