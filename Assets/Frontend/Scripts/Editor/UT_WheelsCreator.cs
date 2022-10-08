using UnityEditor;
using UnityEngine;
using System.Collections.Generic;


public class UT_WheelsCreator : EditorWindow
{

    GameObject wheels_parent_node;
    string name_of_objects;
    bool isnumered;
    float x_position;
     [MenuItem("UT System/Tank helpers/Wheels creator")]
    public static void ShowWindow()
    {
        GetWindowWithRect<UT_WheelsCreator>(new Rect(0,0,350,115), false, "Wheels creator");
    }

    void OnGUI()
    {
        wheels_parent_node = EditorGUILayout.ObjectField("Select a parent node", wheels_parent_node, typeof(GameObject), true) as GameObject;
        name_of_objects = EditorGUILayout.TextField("Wheels name", name_of_objects);
        isnumered = EditorGUILayout.Toggle("Count numbers", isnumered);
        x_position = EditorGUILayout.FloatField("Wheels X position", x_position);
        EditorGUILayout.Space();
        if (GUILayout.Button("Create"))
        {
           if(anyError())
            {
                return;
            }
            int childCount = wheels_parent_node.transform.childCount;
            for(int i=0;i<childCount;i++)
            {
                Transform rootnode = wheels_parent_node.transform.root;
                GameObject actual_child = wheels_parent_node.transform.GetChild(i).gameObject;

                GameObject creatingobj = new GameObject();
                creatingobj.transform.position = actual_child.transform.position;
                creatingobj.transform.rotation = rootnode.rotation;

                creatingobj.name = name_of_objects;
                if (isnumered) creatingobj.name += "(" + (i + 1) + ")";
                creatingobj.transform.SetParent(wheels_parent_node.transform);
                creatingobj.transform.localPosition = new Vector3(x_position, creatingobj.transform.localPosition.y, creatingobj.transform.localPosition.z);

                GameObject creating_holder = new GameObject();
                creating_holder.transform.position = creatingobj.transform.position;
                creating_holder.transform.rotation = creatingobj.transform.rotation;

                creating_holder.name = "HOLDER";
                creating_holder.transform.SetParent(creatingobj.transform);
                creating_holder.transform.localPosition = Vector3.zero;

                actual_child.transform.SetParent(creating_holder.transform);
                creatingobj.transform.SetSiblingIndex(i);
            }

            Debug.Log("[IMC] Created " + childCount+ " wheels");
        }
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
        return false;
    }
}