using UnityEngine;
using UnityEditor;
using System.Collections;
using System;
[CustomEditor(typeof(PathController))]
public class PathEditor : AMazeEditor {

    PathController instance;
    beMobileMaze hostMaze;


    void OnInspectorGUI()
    {
        instance = target as PathController;

        if (instance != null) hostMaze = instance.GetComponentInParent<beMobileMaze>();
            
        if(hostMaze == null) throw new MissingComponentException(string.Format("The Path Controller should be attached to a {0} instance", typeof(beMobileMaze).Name));

        RenderSceneViewUI();

        if (EditorModeProcessEvent != null)
            EditorModeProcessEvent(Event.current);
    }

    void RenderSceneViewUI()
    {
        TileHighlightingOnMouseCursor();
    }
}
