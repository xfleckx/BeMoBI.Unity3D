using UnityEngine;
using UnityEditor;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
public enum PathEditorMode { NONE, PATH_CREATION }

[CustomEditor(typeof(PathInMaze))]
public class PathEditor : AMazeEditor {

    PathInMaze instance;
    beMobileMaze mazePrefab;

    private LinkedList<MazeUnit> pathInSelection;
    private string NameOfCurrentPath = String.Empty;

    private bool PathCreationEnabled; 
    public PathEditorMode ActiveMode { get; set; }
    PathInMaze pathShouldBeRemoved;
    string currentPathName = string.Empty;

    private PathInMaze activePath;

    public void OnEnable()
    {
        instance = target as PathInMaze;

        if (instance == null)
            return;
        if (instance != null){
            maze = instance.GetComponent<beMobileMaze>(); 
        }
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
        instance = target as PathInMaze;

        if (instance != null) {  
            maze = instance.GetComponent<beMobileMaze>();
            mazePrefab = PrefabUtility.GetPrefabParent(maze) as beMobileMaze;
        }
        if(maze == null) throw new MissingComponentException(string.Format("The Path Controller should be attached to a {0} instance", typeof(beMobileMaze).Name));

        base.OnInspectorGUI();

        PathCreationEnabled = GUILayout.Toggle(PathCreationEnabled, "Path creation", EditorStyles.whiteLargeLabel);

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
        Handles.BeginGUI();

        #region Path creation mode

        EditorGUILayout.BeginVertical(GUILayout.Width(100f));

        if (PathCreationEnabled)
        { 
            if (ActiveMode != PathEditorMode.PATH_CREATION)
            { 
                pathInSelection = new LinkedList<MazeUnit>();

                EditorModeProcessEvent += PathCreationMode;
                ActiveMode = PathEditorMode.PATH_CREATION;
            }

            if (GUILayout.Button("Save Path"))
            {
                //if (!instance.Paths.Contains(activePath))
                //{

                //    instance.Paths.Add(activePath);
                //}

                Save(instance);
            }

            GUILayout.Space(4f);


            //if (GUILayout.Button("Clear paths"))
            //{
            //    bool clearAllowed = EditorUtility.DisplayDialog("Delete all paths?", "Do you realy want to delete all paths?", "I agree", "Nooo...");

            //    if (!clearAllowed)
            //        return;

            //    instance.Paths.Clear();
            //    activePath = null;
            //}   

            //if (instance.Paths.Any())
            //{
            //    GUILayout.Space(4f);

            //    GUILayout.Label("Existing Paths");

            //    GUILayout.Space(2f);

            //    foreach (var path in instance.Paths)
            //    {
            //        GUILayout.BeginHorizontal(GUILayout.Width(100f));

            //        var name = path.name != null ? path.name : "no name";

            //        if (GUILayout.Button(name))
            //        {
            //            activePath = path;
            //        }

            //        if (GUILayout.Button("X", GUILayout.Width(20f)))
            //        {
            //            pathShouldBeRemoved = path;
            //        }

            //        GUILayout.EndHorizontal();
            //    }

            //    if (pathShouldBeRemoved != null)
            //    {
            //        //DeletePath(pathShouldBeRemoved);
            //        pathShouldBeRemoved = null;
            //    }
            //}


            //if (GUILayout.Button("Deploy Landmarks"))
            //{

            //}

        }
        else
        {
            EditorModeProcessEvent -= PathCreationMode;

            if (pathInSelection != null)
                pathInSelection.Clear();

        }


        EditorGUILayout.EndVertical();

        #endregion
        Handles.EndGUI();
    }

    private void Save(PathInMaze activePath)
    { 
        EditorUtility.SetDirty(activePath);
    }

    #region path creation logic
    private void PathCreationMode(Event _ce)
    {
        int controlId = GUIUtility.GetControlID(FocusType.Passive);

        if (_ce.type == EventType.MouseDown || _ce.type == EventType.MouseDrag)
        {
            var unit = maze.Grid[Mathf.FloorToInt(currentTilePosition.x), Mathf.FloorToInt(currentTilePosition.y)];
            
            if (_ce.button == 0)
            {
                if (unit == null && !unit.Equals(instance.Units.Last()))
                {
                    Debug.Log("no element added");

                    GUIUtility.hotControl = controlId;
                    _ce.Use();

                    return;
                }

                Debug.Log(string.Format("add {0} to path", unit.name));

                instance.Units.Add(unit);

                instance.GridIDs.Add(unit.GridID);

            }
            if (_ce.button == 1 && instance.Units.Any())
            {
                instance.Units.Remove(unit);

                var lastIndex = activePath.GridIDs.Count - 1;
                instance.GridIDs.RemoveAt(lastIndex);
            } 

            GUIUtility.hotControl = controlId;
            _ce.Use();
        }
    }
     
    public LinkedList<MazeUnit> CreatePathFromGridIDs(LinkedList<Vector2> gridIDs)
    {
        var enumerator = gridIDs.GetEnumerator();
        var units = new LinkedList<MazeUnit>();

        while (enumerator.MoveNext())
        {
            var gridField = enumerator.Current;

            var correspondingUnitHost = maze.transform.FindChild(string.Format("Unit_{0}_{1}", gridField.x, gridField.y));

            if (correspondingUnitHost == null)
                throw new MissingComponentException("It seems, that the path doesn't match the maze! Requested Unit is missing!");

            var unit = correspondingUnitHost.GetComponent<MazeUnit>();

            if (unit == null)
                throw new MissingComponentException("Expected Component on Maze Unit is missing!");

            units.AddLast(unit);
        }

        return units;

    }

    #endregion

    #region path editing logic

    private void PathEditing()
    {

    }

    #endregion 



    protected override void RenderEditorGizmos()
    {
        if (!instance.enabled)
            return; 

        if (instance.Units.Count > 0)
        {
            var iterator = instance.Units.GetEnumerator();
            MazeUnit last = null;

            while (iterator.MoveNext())
            {
                if (last == null)
                {
                    last = iterator.Current;
                    continue;
                }

                var hoveringDistance = new Vector3(0f, maze.RoomHigthInMeter, 0f);

                Gizmos.DrawLine(last.transform.position + hoveringDistance, iterator.Current.transform.position + hoveringDistance);

                last = iterator.Current;
            }
        }
    }

    private bool PathIsValid(LinkedList<MazeUnit> path)
    {
        if (path == null)
            return false;

        bool hasEnoughElements = path.Count > 1;

        return hasEnoughElements;
    }

}
