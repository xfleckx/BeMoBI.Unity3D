using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(ObjectPool))]
public class ObjectPoolEditor : Editor {

    ObjectPool instance;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        instance = target as ObjectPool;

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Open Pool Editor"))
        {

        }

        EditorGUILayout.EndHorizontal();

    }

}
