using UnityEditor;
using UnityEngine;

namespace Frontend.Scripts.Editor
{
    public class UT_DummiesCreator : EditorWindow
    {

        private GameObject wheelsParentNode;
        private string nameOfObjects;
        private bool isNumered;
        private float yOffset; //important -> this is offsetting, not the positioning (relatively to its local Y position we offset the object)
        private Vector2Int childRange;

        [MenuItem("UT System/Tank helpers/Track bones creator")]
        public static void ShowWindow()
        {
            GetWindowWithRect<UT_DummiesCreator>(new Rect(0, 0, 350, 200), false, "Track bones creator");
        }

        private void OnGUI()
        {
            wheelsParentNode = EditorGUILayout.ObjectField("Select a parent node", wheelsParentNode, typeof(GameObject), true) as GameObject;
            nameOfObjects = EditorGUILayout.TextField("Dummies name", nameOfObjects);
            isNumered = EditorGUILayout.Toggle("Count numbers", isNumered);
            yOffset = EditorGUILayout.FloatField("Vertical offset", yOffset);
            childRange = EditorGUILayout.Vector2IntField("Children range", childRange);
            EditorGUILayout.LabelField("X - minimum | Y - maximum");
            EditorGUILayout.Space(); EditorGUILayout.Space();

            if (GUILayout.Button("Create"))
            {
                if (AnyError())
                {
                    return;
                }
                int childCount = wheelsParentNode.transform.childCount;

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
                        int dummiesAmount = (childRange[1] + 1) - childRange[0];
                        for (int i = (childRange[0] - 1); i <= (childRange[1] - 1); i++)
                        {
                            Execute(i, dummiesAmount);
                        }
                        Debug.Log("[IMC] Successfully created " + dummiesAmount + " dummies.");
                    }
                }
                else
                {
                    for (int i = 0; i < childCount; i++)
                    {
                        Execute(i, childCount);
                    }
                    Debug.Log("[IMC] Successfully created " + childCount + " dummies.");
                }
            }
        }
        private void Execute(int i, int count)
        {
            Transform rootNode = wheelsParentNode.transform.root;
            GameObject currentChild = wheelsParentNode.transform.GetChild(i).gameObject;

            GameObject creatingObj = new GameObject();
            creatingObj.transform.SetPositionAndRotation(currentChild.transform.position, rootNode.rotation);

            creatingObj.name = nameOfObjects;
            if (isNumered)
            {
                creatingObj.name += "(" + (i + 1) + ")";
            }
            creatingObj.transform.SetParent(wheelsParentNode.transform);
            creatingObj.transform.localPosition = new Vector3(creatingObj.transform.localPosition.x, creatingObj.transform.localPosition.y + yOffset, creatingObj.transform.localPosition.z);

            GameObject holder = new GameObject();
            holder.transform.SetPositionAndRotation(creatingObj.transform.position, creatingObj.transform.rotation);

            holder.name = "HOLDER";
            holder.transform.SetParent(creatingObj.transform);
            holder.transform.localPosition = Vector3.zero;

            currentChild.transform.SetParent(holder.transform);
            creatingObj.transform.SetSiblingIndex(i);
        }

        private bool AnyError()
        {
            if (string.IsNullOrEmpty(nameOfObjects))
            {
                Debug.LogError("You have to enter the name of objects first!");
                return true;
            }
            if (wheelsParentNode == null)
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
