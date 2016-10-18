using UnityEngine;
using System.Collections;
using UnityEditor;
using System;

public static class EditorViewCompass
{
    private static bool compassAttached = false;

    private static Texture2D compass;

    private const float sizeX = 100;

    private const float sizeY = 100;

    private const float offsetToBorderX = 15;

    private const float offsetToBorderY = 25;
    
    private const string compass_pref_state = "compassAttached";
    
    [InitializeOnLoadMethod]
    public static void EnableCompass()
    {
        bool compassHasBeenAttachedInLastSession = EditorPrefs.GetBool(compass_pref_state);

        if (compassHasBeenAttachedInLastSession) {
            attachCompass();
        }
    }


    private static void attachCompass()
    {
        Debug.Log("Enable Compass");
        LoadTexture();
        SceneView.onSceneGUIDelegate += renderCompassToSceneView;
        compassAttached = true;
        EditorPrefs.SetBool(compass_pref_state, compassAttached);
    }

    [MenuItem("SNEED/Toggle Compass")]
    public static void ToogleCompass()
    {
        compassAttached = EditorPrefs.GetBool(compass_pref_state);

        if (compassAttached) {
            Debug.Log("Disable Compass");
            SceneView.onSceneGUIDelegate -= renderCompassToSceneView;
            compassAttached = false;
            EditorPrefs.SetBool(compass_pref_state, compassAttached);
            UnLoadTexture();
        }
        else {
            attachCompass();
        }

        SceneView.RepaintAll();

    }

    private static void UnLoadTexture()
    {
        Editor.DestroyImmediate(compass);
    }

    private static void LoadTexture()
    {
        compass = new Texture2D(1, 1);
        compass.LoadImage(System.IO.File.ReadAllBytes("Assets/SNEED/Editor/Textures/CompassBackground.png"));
        compass.Apply();

        if (compass == null)
            compass = Texture2D.whiteTexture;
    }

    private static void renderCompassToSceneView(SceneView view)
    {
        if (compass == null)
            LoadTexture();

        var drawingPositionX = view.position.size.x - offsetToBorderX - sizeX ;
        var drawingPositionY = view.position.size.y - offsetToBorderY - sizeY;

        var container = new Rect(drawingPositionX, drawingPositionY, sizeX, sizeY);

        var tempGuiMatrix = GUI.matrix; 
        
        Handles.BeginGUI();
        
        GUILayout.BeginArea(container);

        var cameraAngleOnY = view.camera.transform.rotation.eulerAngles.y;
        
        var pivotX = sizeX / 2;
        var pivotY = (sizeY / 2) * 0.25f; // strange behavior occurs so this magic number is necessary to rotate arount the middle of the texture

        var pivotPoint = new Vector2(pivotX, pivotY);

        GUIUtility.RotateAroundPivot(-cameraAngleOnY, pivotPoint);
        
        GUI.DrawTexture(new Rect(0,0, sizeX, sizeY), compass, ScaleMode.StretchToFill, true, 0);

        GUILayout.EndArea();

        Handles.EndGUI();

        GUI.matrix = tempGuiMatrix;
    }

}
