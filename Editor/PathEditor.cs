using UnityEngine;
using UnityEditor;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
public enum PathEditorMode { NONE, PATH_CREATION }

[CustomEditor(typeof(PathController))]
public class PathEditor : AMazeEditor {

    PathController instance;

    private LinkedList<MazeUnit> pathInSelection;
    private string NameOfCurrentPath = String.Empty;

    private bool PathCreationEnabled; 
    public PathEditorMode ActiveMode { get; set; }
    PathInMaze pathShouldBeRemoved;
    string currentPathName = string.Empty;

    public void OnEnable()
    {
        instance = target as PathController;

        if (instance == null)
            return;
        if (instance != null)
            maze = instance.GetComponent<beMobileMaze>();

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
        Handles.BeginGUI();

        #region Path creation mode

        PathCreationEnabled = GUILayout.Toggle(PathCreationEnabled, "Path creation");

        if (PathCreationEnabled)
        { 
            if (ActiveMode != PathEditorMode.PATH_CREATION)
            { 
                pathInSelection = new LinkedList<MazeUnit>();

                EditorModeProcessEvent += PathCreationMode;
                ActiveMode = PathEditorMode.PATH_CREATION;
            }

            currentPathName = GUILayout.TextField(currentPathName, GUILayout.Width(80f));

            //if (currentPathName != string.Empty && PathIsValid(pathInSelection) && GUILayout.Button("Save Path", GUILayout.Width(75f)))
            //{
            //    var pathInMaze = ScriptableObject.CreateInstance<PathInMaze>();

            //    pathInMaze.PathName = currentPathName;

            //    pathInMaze.Setup(pathInSelection);

            //    if (PathToMazePrefab == string.Empty && PathToMazeCompanionFolder == string.Empty)
            //    {
            //        SavePrefabAndCreateCompanionFolder();
            //    }

            //    if (!AssetDatabase.IsValidFolder(PathToMazeCompanionFolder))
            //    {
            //        PathToMazeCompanionFolder = AssetHelper.GetOrCreateCompanionFolderForPrefab(PathToMazePrefab);
            //    }

            //    AssetDatabase.CreateAsset(pathInMaze, string.Format("{0}/{1}", PathToMazeCompanionFolder, currentPathName));

            //    maze.Paths.Add(pathInMaze);

            //    PrefabUtility.ReplacePrefab(maze.gameObject, referenceToPrefab, ReplacePrefabOptions.ConnectToPrefab);
            //}

            if (instance.Paths.Any())
            {
                GUILayout.Space(4f);

                GUILayout.Label("Existing Paths");

                GUILayout.Space(2f);

                foreach (var path in instance.Paths)
                {
                    GUILayout.BeginHorizontal(GUILayout.Width(100f));

                    if (GUILayout.Button(path.name))
                    {
                        pathInSelection = maze.CreatePathFromGridIDs(path.GridIDs);
                    }

                    if (GUILayout.Button("X", GUILayout.Width(20f)))
                    {
                        pathShouldBeRemoved = path;
                    }

                    GUILayout.EndHorizontal();
                }

                if (pathShouldBeRemoved != null)
                {
                    instance.Paths.Remove(pathShouldBeRemoved);
                    pathShouldBeRemoved = null;
                }
            }
        }
        else
        {
            EditorModeProcessEvent -= PathCreationMode;

            if (pathInSelection != null)
                pathInSelection.Clear();

        }
        #endregion
        Handles.EndGUI();
    }

    #region path creation logic
    private void PathCreationMode(Event _ce)
    {
        int controlId = GUIUtility.GetControlID(FocusType.Passive);

        if (_ce.type == EventType.MouseDown || _ce.type == EventType.MouseDrag)
        {
            //var unitHost = GameObject.Find(string.Format(maze.UnitNamePattern, currentTilePosition.x, currentTilePosition.y));

            var unit = maze.Grid[Mathf.FloorToInt(currentTilePosition.x), Mathf.FloorToInt(currentTilePosition.y)];

            if (unit)
            {
                Debug.Log(string.Format("add {0} to path", unit.name));
                pathInSelection.AddLast(unit);

                if (_ce.shift && pathInSelection.Any())
                {
                    pathInSelection.Remove(unit);
                }
            
            }
            else
            {
                Debug.Log("no element added");
            }

            GUIUtility.hotControl = controlId;
            _ce.Use();
        }
    }

    #endregion

    protected override void RenderEditorGizmos()
    {
        if (pathInSelection != null)
        {
            var iterator = pathInSelection.GetEnumerator();
            MazeUnit last = null;

            while (iterator.MoveNext())
            {
                if (!last)
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
