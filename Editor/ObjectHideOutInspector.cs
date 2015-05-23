using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(ObjectHideOut))]
public class ObjectHideOutInspector : Editor {

    ObjectHideOut instance;

    public override void OnInspectorGUI()
    {
        instance = target as ObjectHideOut;

        base.OnInspectorGUI();

        EditorGUILayout.BeginHorizontal();

        EditorGUILayout.BeginVertical();

        if (GUILayout.Button(MazeUnit.NORTH))
        {
            Toogle(instance, MazeUnit.NORTH);
        }

        if (GUILayout.Button(MazeUnit.SOUTH))
        {
            Toogle(instance, MazeUnit.SOUTH);
        }

        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical();

        if (GUILayout.Button(MazeUnit.WEST))
        {
            Toogle(instance, MazeUnit.WEST);
        }

        if (GUILayout.Button(MazeUnit.EAST))
        {
            Toogle(instance, MazeUnit.EAST);
        }
        EditorGUILayout.EndVertical();


        EditorGUILayout.EndHorizontal();

    }

    private void Toogle(ObjectHideOut hideOut, string directionName)
    {
        if (hideOut.IsOpen(directionName))
            hideOut.Close(directionName);
        else
            hideOut.Open(directionName);

    }
}
