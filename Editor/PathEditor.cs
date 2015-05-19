using UnityEngine;
using UnityEditor;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
public enum PathEditorMode { NONE, PATH_CREATION }

[CustomEditor(typeof(PathController))]
public class PathEditor : AMazeEditor {

    PathController instance;
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
        instance = target as PathController;

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
        instance = target as PathController;

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

    private string activePathId = string.Empty;

    public override void RenderSceneViewUI()
    {
        Handles.BeginGUI();


        activePathId = activePath != null ? activePath.name : "No active path";

        GUILayout.Label(activePathId, EditorStyles.whiteBoldLabel);      

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

            //if (currentPathName != string.Empty && PathIsValid(pathInSelection) && GUILayout.Button("Save Path", GUILayout.Width(75f)))
            //{
            //    var pathInMaze = ScriptableObject.CreateInstance<PathInMaze>();

            //    pathInMaze.Identifier = currentPathName;

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

            if (GUILayout.Button("New Path"))
            {
                activePath = CreateNewPath();
            }

            if(activePath != null)
                activePath.name = GUILayout.TextField(activePath.name);

            if (GUILayout.Button("Save Path"))
            {
                if (!instance.Paths.Contains(activePath)) {
                    
                    instance.Paths.Add(activePath);
                }

                Save(activePath);

                EditorUtility.SetDirty(instance);
            }

            GUILayout.Space(4f);

            if (GUILayout.Button("Delete Path"))
            {
                DeletePath(activePath);
            }

            if (GUILayout.Button("Clear paths"))
            {
                bool clearAllowed = EditorUtility.DisplayDialog("Delete all paths?", "Do you realy want to delete all paths?", "I agree", "Nooo...");

                if (!clearAllowed)
                    return;

                instance.Paths.ForEach((p) => DeletePath(p));
                instance.Paths.Clear();
                activePath = null;
            }   

            if (instance.Paths.Any())
            {
                GUILayout.Space(4f);

                GUILayout.Label("Existing Paths");

                GUILayout.Space(2f);

                foreach (var path in instance.Paths)
                {
                    GUILayout.BeginHorizontal(GUILayout.Width(100f));

                    var name = path.name != null ? path.name : "no name";

                    if (GUILayout.Button(name))
                    {
                        activePath = path;
                    }

                    if (GUILayout.Button("X", GUILayout.Width(20f)))
                    {
                        pathShouldBeRemoved = path;
                    }

                    GUILayout.EndHorizontal();
                }

                if (pathShouldBeRemoved != null)
                {
                    DeletePath(pathShouldBeRemoved);
                    pathShouldBeRemoved = null;
                }
            }


            if (GUILayout.Button("Deploy Landmarks"))
            {

            }

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
        if (!EditorUtility.IsPersistent(activePath))
        {
            CreatePathAsset(activePath);
        }
        else
        {
            EditorUtility.SetDirty(activePath);
        }
    }

    private PathInMaze CreateNewPath()
    {
      var newPath = ScriptableObject.CreateInstance<PathInMaze>();
      int pathNumber = instance.Paths.Count;
      newPath.name = string.Format("p_{0}", pathNumber);
      instance.Paths.Add(newPath);
      return newPath;
    }

    private void CreatePathAsset(PathInMaze path)
    {
        var mazePrefab = PrefabUtility.GetPrefabParent(maze);

        var prefabPath = AssetDatabase.GetAssetPath(mazePrefab);
        var lastSeparator = prefabPath.LastIndexOf('.');
        prefabPath = prefabPath.Substring(0, lastSeparator);

        var fileName = Path.Combine(prefabPath, string.Format("{0}.{1}", path.name, "asset"));

        AssetDatabase.CreateAsset(path, fileName);

        foreach (var item in path.Units)
        {
            AssetDatabase.AddObjectToAsset(item, path);
        }
    }

    private void DeletePath(PathInMaze path)
    {
        if (activePath != null && activePath.Equals(path))
            activePath = null; 

        instance.Paths.Remove(path);
        DestroyImmediate(path);
    }

    #region path creation logic
    private void PathCreationMode(Event _ce)
    {
        int controlId = GUIUtility.GetControlID(FocusType.Passive);

        if (_ce.type == EventType.MouseDown || _ce.type == EventType.MouseDrag)
        {
            var prefabParent = PrefabUtility.GetPrefabParent(maze);

            var prefabLocation = AssetDatabase.GetAssetPath(prefabParent);

            var prefabLoad = AssetDatabase.LoadAssetAtPath(prefabLocation, typeof(beMobileMaze));

            var unit = maze.Grid[Mathf.FloorToInt(currentTilePosition.x), Mathf.FloorToInt(currentTilePosition.y)];
            
            if (_ce.button == 0)
            {
                if (unit == null && !unit.Equals(activePath.Units.Last()))
                {
                    Debug.Log("no element added");

                    GUIUtility.hotControl = controlId;
                    _ce.Use();

                    return;
                }

                Debug.Log(string.Format("add {0} to path", unit.name));

                activePath.Units.Add(unit);
            }

            if (_ce.button == 1 && activePath.Units.Any())
            {
                activePath.Units.Remove(unit);
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
        if (activePath == null)
            MarkerColor = Color.gray;
        
        if (activePath != null && activePath.Units.Count > 0)
        {
            var iterator = activePath.Units.GetEnumerator();
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
