using UnityEngine;
using UnityEditor;
using System.Collections;
using System;
[CustomEditor(typeof(PathController))]
public class PathEditor : AMazeEditor {

    PathController instance; 

    public void OnEnable()
    {
        instance = target as PathController;

        if (instance == null)
            return;
         
        instance.EditorGizmoCallbacks += RenderTileHighlighting;
        instance.EditorGizmoCallbacks += RenderEditorGizmos; 
    }

    public void OnDisable()
    {
        if (instance == null)
            return;

        instance.EditorGizmoCallbacks -= RenderTileHighlighting;
        instance.EditorGizmoCallbacks -= RenderEditorGizmos; 
    }

    public override void OnInspectorGUI()
    {
        instance = target as PathController;

        if (instance != null) 
            maze = instance.GetComponent<beMobileMaze>();
            
        if(maze == null) throw new MissingComponentException(string.Format("The Path Controller should be attached to a {0} instance", typeof(beMobileMaze).Name));

        base.OnInspectorGUI();

        RenderSceneViewUI();

        if (EditorModeProcessEvent != null)
            EditorModeProcessEvent(Event.current);
    }

    protected new void RenderTileHighlighting()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(MarkerPosition + new Vector3(0, maze.RoomHigthInMeter / 2, 0), new Vector3(maze.RoomDimension.x, maze.RoomHigthInMeter, maze.RoomDimension.z) * 1.1f);
    }

    public override void RenderSceneViewUI()
    {

    }

    protected override void RenderEditorGizmos()
    {
        
    }
}
