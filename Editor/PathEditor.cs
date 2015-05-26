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

    private string pathElementPattern = "{0} {1} = {2} turn {3}";

    private bool PathCreationEnabled; 
    public PathEditorMode ActiveMode { get; set; }
    PathInMaze pathShouldBeRemoved;
    string currentPathName = string.Empty;
      
    private bool showElements;

    public void OnEnable()
    {
        instance = target as PathInMaze;

        if (instance == null)
            return;
        if (instance != null){
            maze = instance.GetComponent<beMobileMaze>(); 
        }

        if(instance.PathElements == null)
            instance.PathElements = new Dictionary<Vector2, PathElement>();

        instance.EnableHideOut();

        instance.EditorGizmoCallbacks += RenderTileHighlighting;
        instance.EditorGizmoCallbacks += RenderEditorGizmos; 
    }

    public void OnDisable()
    {
        if (instance == null)
            return;

        instance.DisableHideOut();

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

        EditorGUILayout.BeginVertical();

        if (GUILayout.Button("Reverse Path"))
        {
            IEnumerable<KeyValuePair<Vector2, PathElement>> temp = instance.PathElements.Reverse().ToList();
            instance.PathElements = temp.ToDictionary((kvp) => kvp.Key, (kvp) => kvp.Value);

        }

        showElements = EditorGUILayout.Foldout(showElements, "Show Elements");
        
        if(showElements)
            RenderElements();


        PathCreationEnabled = GUILayout.Toggle(PathCreationEnabled, "Path creation");
        
        if (GUILayout.Button("Deploy Landmarks"))
        {

        }

        if (instance.HideOut == null && GUILayout.Button("Deploy Object HideOut"))
        {
            DeployObjectHideOut();
        }

        if (instance.HideOut != null && GUILayout.Button("Remove Object HideOut"))
        {
            RemoveObjectHideOut();
        }

        if (instance.HideOut != null && instance.HideOut.enabled && GUILayout.Button("Hide HideOut"))
        {
            instance.DisableHideOut();
        }

        if (instance.HideOut != null && !instance.HideOut.enabled && GUILayout.Button("Show HideOut"))
        {
            instance.EnableHideOut();
        }

        EditorGUILayout.EndVertical();

        if (EditorModeProcessEvent != null)
            EditorModeProcessEvent(Event.current);
    }

    private void RenderElements()
    {
        EditorGUILayout.BeginVertical();

        if (GUILayout.Button("Save Path"))
        {
            Save(instance);
        }

        foreach (var e in instance.PathElements)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(
                string.Format(pathElementPattern, e.Key.x, e.Key.y, Enum.GetName(typeof(UnitType), e.Value.Type), Enum.GetName(typeof(TurnType), e.Value.Turn)), GUILayout.Width(150f));
            
            EditorGUILayout.ObjectField(e.Value.Unit, typeof(MazeUnit), false);
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndVertical();
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

            GUILayout.Space(4f);
            
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

            if (unit == null)
            {
                Debug.Log("no element added");

                GUIUtility.hotControl = controlId;
                _ce.Use();

                return;
            }

            if (_ce.button == 0)
            {
                Add(unit);

                EditorUtility.SetDirty(instance);
            }
            if (_ce.button == 1 && instance.PathElements.Any())
            {
                Remove(unit);

                EditorUtility.SetDirty(instance);
            } 

            GUIUtility.hotControl = controlId;
            _ce.Use();
        }
    }

    private void Add(MazeUnit newUnit)
    {
        Debug.Log(string.Format("add {0} to path", newUnit.name));

        if (instance.PathElements.ContainsKey(newUnit.GridID))
            return;

        var newElement = new PathElement(newUnit);
        
        newElement = GetElementType(newElement);

        if (newElement.Type == UnitType.L || newElement.Type == UnitType.T || newElement.Type == UnitType.X) {
            var previousElement = instance.PathElements.Values.Last();
            newElement = GetTurnType(newElement, previousElement);

            DeployLandmark(previousElement);
        }
         

        instance.PathElements.Add(newUnit.GridID, newElement);
    }

    private void DeployLandmark(PathElement previousElement)
    {
        
    }

    private void DeployObjectHideOut()
    {
        var pathEnd = instance.PathElements.Last();

        //var obj = Resources.Load(unitPrefabName); // Load from local asset path 
        var obj = AssetDatabase.LoadAssetAtPath("Assets/beMobi.Unity3D/Prefabs/ObjectHideOut.prefab", typeof(GameObject));

        if (obj == null)
        {
            Debug.LogError("ObjectHideOut.prefab not found!");
        }

        var hideOutHost = PrefabUtility.InstantiatePrefab(obj) as GameObject;
        var hideOut = hideOutHost.GetComponent<ObjectHideOut>();

        pathEnd.Value.Unit.gameObject.SetActive(false);
        instance.HideOut = hideOut;
        instance.HideOutReplacement = pathEnd.Value.Unit;
        hideOut.transform.parent = instance.transform;
        hideOut.transform.localScale = pathEnd.Value.Unit.transform.localScale;
        hideOut.transform.position = pathEnd.Value.Unit.transform.position;

    }

    private void RemoveObjectHideOut()
    {
        if (instance.HideOut != null)
        {
            var pathEnd = instance.HideOutReplacement;

            DestroyImmediate(instance.HideOut.gameObject);
            instance.HideOut = null;

            pathEnd.gameObject.SetActive(true);
        }


    }

    public static PathElement GetElementType(PathElement element)
    {
        var u = element.Unit;

        if (u.WaysOpen == (OpenDirections.East | OpenDirections.West) ||
            u.WaysOpen == (OpenDirections.North | OpenDirections.South) || 
            u.WaysOpen == OpenDirections.East ||
            u.WaysOpen == OpenDirections.West ||
            u.WaysOpen == OpenDirections.North ||
            u.WaysOpen == OpenDirections.South )
        {
           element.Type = UnitType.I;
        }
        
        if(u.WaysOpen == OpenDirections.All)
            element.Type = UnitType.X;

        if(u.WaysOpen == (OpenDirections.West | OpenDirections.North | OpenDirections.East) ||
           u.WaysOpen ==  (OpenDirections.West | OpenDirections.South | OpenDirections.East) ||
           u.WaysOpen == (OpenDirections.West | OpenDirections.South | OpenDirections.North) ||
            u.WaysOpen ==  (OpenDirections.East | OpenDirections.South | OpenDirections.North) )
        {
            element.Type = UnitType.T;
        }

        if(u.WaysOpen == (OpenDirections.West | OpenDirections.North ) ||
           u.WaysOpen ==  (OpenDirections.West | OpenDirections.South ) ||
           u.WaysOpen == (OpenDirections.East | OpenDirections.South ) ||
            u.WaysOpen ==  (OpenDirections.East | OpenDirections.North) )
        {
            element.Type = UnitType.L;
        }

        return element;
    }

    public static PathElement GetTurnType(PathElement current, PathElement last)
    {  
        

        return current;
    }

    private void Remove(MazeUnit unit)
    {
        instance.PathElements.Remove(unit.GridID);
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

    protected override void RenderEditorGizmos()
    {
        if (!instance.enabled)
            return;

        if (instance.PathElements.Count > 0)
        {
            var hoveringDistance = new Vector3(0f, maze.RoomHigthInMeter, 0f);

            var start = instance.PathElements.First().Value.Unit.transform;
            Handles.color = Color.blue;
            Handles.CubeCap(this.GetInstanceID(), start.position + hoveringDistance, start.rotation, 0.3f);


            var iterator = instance.PathElements.Values.GetEnumerator();
            MazeUnit last = null;

            while (iterator.MoveNext())
            {
                if (last == null)
                {
                    last = iterator.Current.Unit;
                    continue;
                }

                Gizmos.DrawLine(last.transform.position + hoveringDistance, iterator.Current.Unit.transform.position + hoveringDistance);

                last = iterator.Current.Unit;
            }


            var end = instance.PathElements.Last().Value.Unit.transform;
            Handles.ConeCap(this.GetInstanceID(), end.position + hoveringDistance, start.rotation, 0.3f);
        }
    }

}
