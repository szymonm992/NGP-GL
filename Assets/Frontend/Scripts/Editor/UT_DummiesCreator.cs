using UnityEditor;
using UnityEngine;

public class UT_DummiesCreator : EditorWindow
{

    GameObject wheels_parent_node;
    string name_of_objects;
    bool isnumered;
    float Y_offset; //important -> this is offsetting, not the positioning (relatively to its local Y position we offset the object)
    Vector2Int child_range;

    [MenuItem("UT System/Tank helpers/Track bones creator")]
    public static void ShowWindow()
    {
        GetWindowWithRect<UT_DummiesCreator>(new Rect(0, 0, 350, 200), false, "Track bones creator");
    }


    void OnGUI()
    {
        wheels_parent_node = EditorGUILayout.ObjectField("Select a parent node", wheels_parent_node, typeof(GameObject), true) as GameObject;
        name_of_objects = EditorGUILayout.TextField("Dummies name", name_of_objects);
        isnumered = EditorGUILayout.Toggle("Count numbers", isnumered);
        Y_offset = EditorGUILayout.FloatField("Vertical offset", Y_offset);
        child_range = EditorGUILayout.Vector2IntField("Children range", child_range);
        EditorGUILayout.LabelField("X - minimum | Y - maximum");
        EditorGUILayout.Space(); EditorGUILayout.Space(); 
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
                    int howmany = (child_range[1]+1) - child_range[0];
                    for (int i = (child_range[0]-1); i <= (child_range[1]-1); i++)
                    {
                        Execute(i, howmany);
                    }
                    Debug.Log("[IMC] Successfully created " + howmany + " dummies.");
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


    void Execute(int i, int count)
    {
        Transform rootnode = wheels_parent_node.transform.root;
        GameObject actual_child = wheels_parent_node.transform.GetChild(i).gameObject;

        GameObject creatingobj = new GameObject();
        creatingobj.transform.position = actual_child.transform.position;
        creatingobj.transform.rotation = rootnode.rotation;

        creatingobj.name = name_of_objects;
        if (isnumered) creatingobj.name += "(" + (i + 1) + ")";
        creatingobj.transform.SetParent(wheels_parent_node.transform);
        creatingobj.transform.localPosition = new Vector3(creatingobj.transform.localPosition.x, creatingobj.transform.localPosition.y+Y_offset, creatingobj.transform.localPosition.z);

        GameObject creating_holder = new GameObject();
        creating_holder.transform.position = creatingobj.transform.position;
        creating_holder.transform.rotation = creatingobj.transform.rotation;

        creating_holder.name = "HOLDER";
        creating_holder.transform.SetParent(creatingobj.transform);
        creating_holder.transform.localPosition = Vector3.zero;

        actual_child.transform.SetParent(creating_holder.transform);
        creatingobj.transform.SetSiblingIndex(i);
        //Debug.Log("actual_child"+actual_child.name);
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
        if(child_range[0]<0 || child_range[1] < 0)
        {
            Debug.LogError("Incorrect range values!");
            return true;
        }
        
        return false;
    }
}
