using UnityEngine;
using System.Collections;
using UnityEditor;
using UnityEngine.UI;

[CustomEditor(typeof(VirtualRealityManager))]
public class VRManagerInspector : Editor
{
    private bool showHUDElementReferences = false;

    private VirtualRealityManager vrcontroller;

    public override void OnInspectorGUI()
    {
        vrcontroller = (VirtualRealityManager)target;

        base.OnInspectorGUI();

        EditorGUILayout.BeginVertical();

        EditorGUILayout.EndVertical();

    }

    public void OnSceneGUI()
    { 
        Handles.BeginGUI();

        GUILayout.Space(25);

        GUILayout.BeginVertical(GUILayout.Width(75));

        GUILayout.Label("Choose Environment", EditorStyles.whiteLabel);
        GUILayout.Space(10);
        foreach (var item in vrcontroller.Environments)
        {
            if (GUILayout.Button(item.Title))
            {
                vrcontroller.ChangeWorld(item.Title);
            }
        }
        GUILayout.EndVertical();


        Handles.EndGUI();
    }

}
