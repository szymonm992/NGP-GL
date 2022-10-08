using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


public class UT_MultiRename : EditorWindow
{


    GameObject wheels_parent_node;
    string name_of_objects;
    bool isnumered;
    Vector2Int child_range;

    [MenuItem("UT System/Addons/Multiple name changer")]
    public static void ShowWindow()
    {
        GetWindowWithRect<UT_MultiRename>(new Rect(0, 0, 350, 155), false, "Multiple name changer");
    }
    void OnGUI()
    {
        wheels_parent_node = EditorGUILayout.ObjectField("Select a parent node", wheels_parent_node, typeof(GameObject), true) as GameObject;
        name_of_objects = EditorGUILayout.TextField("Objects name", name_of_objects);
        isnumered = EditorGUILayout.Toggle("Count numbers", isnumered);
        child_range = EditorGUILayout.Vector2IntField("Children range", child_range);
        EditorGUILayout.LabelField("X - minimum | Y - maximum");
        EditorGUILayout.Space(); 
        if (GUILayout.Button("Create"))
        {

            if (anyError())
            {
                return;
            }
            int childCount = wheels_parent_node.transform.childCount;

            if (child_range[0] > 0 && child_range[1] > 0)
            {
                if (child_range[1] > childCount)
                {
                    Debug.LogError("Incorrect maximum range!");
                    return;
                }
                else if (child_range[0] > child_range[1])
                {
                    Debug.LogError("Incorrect minimal range!");
                }
                else
                {
                    int howmany = (child_range[1] + 1) - child_range[0];
                    for (int i = (child_range[0] - 1); i <= (child_range[1] - 1); i++)
                    {
                        Execute(i, howmany);
                    }
                    Debug.Log("[IMC] Renamed successfully " + howmany + " objects");
                }
            }
            else
            {
                for (int i = 0; i < childCount; i++)
                {
                    Execute(i, childCount);
                }
                Debug.Log("[IMC] Renamed successfully "+childCount+" objects");
            }

          
        }
    }

    void Execute(int i, int count)
    {
        GameObject actual_child = wheels_parent_node.transform.GetChild(i).gameObject;

        actual_child.name = name_of_objects;
        if (isnumered) actual_child.name += "(" + (i + 1) + ")";
    }


    bool anyError()
    {
        if (string.IsNullOrEmpty(name_of_objects))
        {
            Debug.LogError("You have to enter the name of objects first!");
            return true;
        }
        if (wheels_parent_node == null)
        {
            Debug.LogError("You have to select a parent node first!");
            return true;
        }
        if (child_range[0] < 0 || child_range[1] < 0)
        {
            Debug.LogError("Incorrect range values!");
            return true;
        }

        return false;
    }
}
