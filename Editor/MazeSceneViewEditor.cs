using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

class MazeSceneViewEditor
{
    [MenuItem("Window/MazeEditor/Enable")]
    public static void Enable()
    {
        SceneView.onSceneGUIDelegate += ProvideMergeOptionsOn;
        Debug.Log("MazeEditor Scene GUI : Enabled");
    }

    [MenuItem("Window/MazeEditor/Disable")]
    public static void Disable()
    {
        SceneView.onSceneGUIDelegate -= ProvideMergeOptionsOn;
        Debug.Log("MazeEditor Scene GUI : Disabled");
    }

    private static void ProvideMergeOptionsOn(SceneView sceneview)
    {
        bool drawMergeOptions = false ;

        Vector2 menuPosition = new Vector2(0,0);

        if (Selection.gameObjects.Count() > 1 && 
            Selection.gameObjects.All(
                (go) => go.GetComponent<MazeUnit>())
            )
        {
            drawMergeOptions = true;

            menuPosition = sceneview.camera.WorldToScreenPoint(
                                     Selection.gameObjects.First().transform.position); 
        }

        Handles.BeginGUI();



        if (drawMergeOptions)
        {
            if (GUI.Button(new Rect(menuPosition.x, menuPosition.y, 50, 15), "Join"))
            {
                   MazeUnit.Join(Selection.gameObjects.Select(
                                    (go) => go.GetComponent<MazeUnit>()));
            }
            
            if (GUI.Button(new Rect(menuPosition.x, menuPosition.y + 15, 50, 15), "Split"))
            {
                MazeUnit.Split(Selection.gameObjects.Select(
                                   (go) => go.GetComponent<MazeUnit>()));
            }
        } 
        
        Handles.EndGUI();
    }
 
}