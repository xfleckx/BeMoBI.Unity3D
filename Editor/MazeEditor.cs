using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

public enum MazeEditorMode {NONE, EDITING, SELECTION, PATH_CREATION}

[CustomEditor(typeof(beMobileMaze))]
public class MazeEditor : AMazeEditor
{
    private const string unitPrefabName = "MazeUnit";
     
    public float unitFloorOffset = 0f;


    private bool PathCreationEnabled = false;
    private HashSet<GameObject> currentSelection;
    private LinkedList<MazeUnit> pathInSelection;
    private string NameOfCurrentPath = String.Empty;

    private string PathToMazePrefab = string.Empty;
    private string PathToMazeCompanionFolder = string.Empty;

    private bool SelectionModeEnabled = false;

    private bool EditingModeEnabled = false;
    private bool modeAddEnabled = false;
    private bool modeRemoveEnabled = false;
    private bool DisconnectFromUnitPrefab = true;

    private string ObjectFolderName = string.Empty;

    private UnityEngine.Object referenceToPrefab;

    private MazeEditorMode ActiveMode = MazeEditorMode.NONE;

    private MazeUnit lastAddedUnit;

    private GUIStyle sceneViewEditorStyle;
      
    public void OnEnable()
    { 
        maze = (beMobileMaze)target;

        referenceToPrefab = PrefabUtility.GetPrefabParent(maze.gameObject);

        MazeEditorUtil.ReconfigureGrid(maze, maze.MazeWidthInMeter, maze.MazeLengthInMeter);

        if (referenceToPrefab != null) { 
            PathToMazePrefab = AssetDatabase.GetAssetPath(referenceToPrefab);

            PathToMazeCompanionFolder = AssetHelper.GetOrCreateCompanionFolderForPrefab(PathToMazePrefab);
        }

        sceneViewEditorStyle = new GUIStyle();

        if (maze) {
            maze.EditorGizmoCallbacks += RenderTileHighlighting;
            maze.EditorGizmoCallbacks += RenderEditorGizmos;
        }
    }

    public void OnDisable()
    {
        if (maze) {
            maze.EditorGizmoCallbacks -= RenderTileHighlighting;
            maze.EditorGizmoCallbacks -= RenderEditorGizmos;
        }
    }

    protected override void OnHeaderGUI()
    {
        //base.OnHeaderGUI();
    }

    public override void OnInspectorGUI()
    {
        if (maze == null) {
            renderEmptyMazeGUI();
        }

        GUILayout.BeginVertical();
        
        GUILayout.BeginHorizontal();
        GUILayout.Label("Length of Maze");
        maze.MazeLengthInMeter = EditorGUILayout.FloatField(maze.MazeLengthInMeter, GUILayout.Width(50));
        GUILayout.Label("m");
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Width of Maze");
        maze.MazeWidthInMeter = EditorGUILayout.FloatField(maze.MazeWidthInMeter, GUILayout.Width(50));
        GUILayout.Label("m");
        GUILayout.EndHorizontal();

        if (GUILayout.Button("Configure Grid"))
        {
            MazeEditorUtil.ReconfigureGrid(maze, maze.MazeWidthInMeter, maze.MazeLengthInMeter);
        }

        GUILayout.BeginHorizontal();

        maze.RoomDimension = EditorGUILayout.Vector3Field("Room Dimension", maze.RoomDimension);
        
        if (GUILayout.Button("Rescale"))
        {
            Rescale(maze, maze.RoomDimension);
        }

        GUILayout.EndHorizontal();


        GUILayout.BeginHorizontal();
        GUILayout.Label("Height of Rooms");
        maze.RoomHigthInMeter = EditorGUILayout.FloatField(maze.RoomHigthInMeter, GUILayout.Width(50));
        GUILayout.Label("m");
        GUILayout.EndHorizontal();
        
        GUILayout.BeginHorizontal();
        maze.UnitNamePattern = EditorGUILayout.TextField("Unit Name Pattern", maze.UnitNamePattern);
        GUILayout.EndHorizontal();

        GUILayout.EndVertical();

        GUILayout.BeginVertical();

        if (GUILayout.Button("Open Maze Editor", GUILayout.Width(255)))
        {
            MazeEditorWindow window = (MazeEditorWindow)EditorWindow.GetWindow(typeof(MazeEditorWindow));
            window.Init(maze);
        }

        if (GUILayout.Button("Clone Maze", GUILayout.Width(255)))
        {
            var clone = GameObject.Instantiate(maze);
        }

        if (GUILayout.Button("Create Maze Prefab", GUILayout.Width(255)))
        {
            SavePrefabAndCreateCompanionFolder();
        }

        if (referenceToPrefab && GUILayout.Button("Update Prefab"))
        {
            UpdatePrefabOfCurrentMaze();
        }

        if (GUILayout.Button("Show Paths", GUILayout.Width(255)))
        {

        }

        GUILayout.Space(5f);

        GUILayout.BeginHorizontal();
        ObjectFolderName = GUILayout.TextField(ObjectFolderName, GUILayout.Width(150f));
        
        if(GUILayout.Button("...")){
            ObjectFolderName = EditorUtility.OpenFolderPanel("Open folder containing objects", "Assets", "");
        }

        GUILayout.EndHorizontal();
        GUILayout.EndVertical();

    }

    private void Rescale(beMobileMaze focusedMaze, Vector3 newUnitScale)
    {
        var origin = focusedMaze.transform.position;

        foreach (var item in focusedMaze.Units)
        {
            item.transform.localScale = newUnitScale;

            InitializeUnit(focusedMaze, item.GridID, item.gameObject); 
        }
    }

    private void UpdatePrefabOfCurrentMaze()
    {
       referenceToPrefab = PrefabUtility.ReplacePrefab(maze.gameObject, referenceToPrefab, ReplacePrefabOptions.ConnectToPrefab);
    }

    private void SavePrefabAndCreateCompanionFolder()
    {
        PathToMazePrefab = EditorUtility.SaveFilePanelInProject("Save maze", "maze.prefab", "prefab", "Save maze as Prefab");
        Debug.Log("Saved to " + PathToMazePrefab);
        referenceToPrefab = PrefabUtility.CreatePrefab(PathToMazePrefab, maze.gameObject, ReplacePrefabOptions.ConnectToPrefab);

        PathToMazeCompanionFolder = AssetHelper.GetOrCreateCompanionFolderForPrefab(PathToMazePrefab);

        Debug.Log("Create companion folder " + PathToMazePrefab);
    }

    #region Editor Modes

    private void EditingMode(Event _ce)
    {
        int controlId = GUIUtility.GetControlID(FocusType.Passive);
        // Before repaint
        if (_ce.type == EventType.Layout || _ce.type == EventType.layout)
        {

        }


        if (_ce.type == EventType.Repaint || _ce.type == EventType.repaint)
        {
        }



        if (_ce.type == EventType.MouseDown || _ce.type == EventType.MouseDrag)
        {
            if (EditingModeEnabled)
            {
                if (modeAddEnabled)
                    Draw();
                else if (modeRemoveEnabled)
                    Erase();
            }
            GUIUtility.hotControl = controlId;
            _ce.Use();
        }
    }

    private void Draw()
    { 
        // Given the tile position check to see if a tile has already been created at that location
        var unitHost = GameObject.Find(string.Format(maze.UnitNamePattern, currentTilePosition.x, currentTilePosition.y));

        // if there is already a tile present and it is not a child of the game object we can just exit.
        if (unitHost != null && unitHost.transform.parent != maze.transform)
        {
            return;
        }

        // if no game object was found create the unitHost prefab
        if (unitHost == null)
        {
            //var obj = Resources.Load(unitPrefabName); // Load from local asset path 
            var obj = AssetDatabase.LoadAssetAtPath("Assets/beMobi.Unity3D/Prefabs/MazeUnit.prefab", typeof(GameObject));
             
            if (obj)
            {
                unitHost = PrefabUtility.InstantiatePrefab(obj) as GameObject;
                PrefabUtility.DisconnectPrefabInstance(unitHost);
            }
            else
            {
                Debug.LogError(string.Format("Prefab \"{0}\" not found in resources", unitPrefabName));
                return;
            }
        }

        var unit = this.InitializeUnit(maze, currentTilePosition, unitHost);
        maze.Grid[(int)currentTilePosition.x, (int)currentTilePosition.y] = unit;
        maze.Units.Add(unit);

    }

    private MazeUnit InitializeUnit(beMobileMaze mazeHost, Vector2 tilePos, GameObject unit)
    {
        var tilePositionInLocalSpace = new Vector3(
            (tilePos.x * mazeHost.RoomDimension.x) + (mazeHost.RoomDimension.x / 2f),
            unitFloorOffset,
            (tilePos.y * mazeHost.RoomDimension.z) + (mazeHost.RoomDimension.z / 2f));

        unit.transform.position = mazeHost.transform.position + tilePositionInLocalSpace;

        // we scale the unit to the tile size defined by the TileMap.TileWidth and TileMap.TileHeight fields 
        unit.transform.localScale = new Vector3(mazeHost.RoomDimension.x, mazeHost.RoomDimension.y, mazeHost.RoomDimension.z);

        // set the cubes parent to the game object for organizational purposes
        unit.transform.parent = mazeHost.transform;

        // give the unit a assetName that represents it's location within the tile mazeHost
        unit.name = string.Format(maze.UnitNamePattern, tilePos.x, tilePos.y);

        MazeUnit mazeUnit = unit.GetComponent<MazeUnit>();

        mazeUnit.Initialize(tilePos);

        return mazeUnit;
    }

    /// <summary>
    /// Erases a block at the pre-calculated mouse hit position
    /// </summary>
    private void Erase()
    {
        var unitHost = GameObject.Find(string.Format(maze.UnitNamePattern, currentTilePosition.x, currentTilePosition.y));

        if (!unitHost)
        {
            Debug.Log("Nothing to erase!");
            return;
        }

        var unit = unitHost.GetComponent<MazeUnit>();

        // if a game object was found with the same assetName and it is a child we just destroy it immediately
        if (unit != null && unit.transform.parent == maze.transform)
        {
            maze.Units.Remove(unit);
            maze.Grid[(int)currentTilePosition.x, (int)currentTilePosition.y] = null;
            DestroyImmediate(unit.gameObject);
        }
    }

    private void SelectionMode(Event _ce)
    {
        int controlId = GUIUtility.GetControlID(FocusType.Passive);

        if (_ce.type == EventType.MouseDown || _ce.type == EventType.MouseDrag) {

            var unitHost = GameObject.Find(string.Format(maze.UnitNamePattern, currentTilePosition.x, currentTilePosition.y));

            if (unitHost != null)
            {
                if(currentSelection.Contains(unitHost)){
                    if (_ce.button == 1)
                        currentSelection.Remove(unitHost);
                }
                else { 
                    if(_ce.button == 0)
                        currentSelection.Add(unitHost);
                }
            } 

            GUIUtility.hotControl = controlId;
            _ce.Use();
        }
        
    }

    private void TryConnectingCurrentSelection()
    {
        if (currentSelection == null)
            return;

        if (!currentSelection.Any())
            return;

        var iterator = currentSelection.GetEnumerator();

        MazeUnit last = null;

        while (iterator.MoveNext())
        {
            var current = iterator.Current.GetComponent<MazeUnit>();

            if (!last)
            {
                last = current;
                continue;
            }

            if (current.GridID.x - 1 == last.GridID.x)
            {
                last.Open(MazeUnit.EAST);
                current.Open(MazeUnit.WEST);
            }
            else if (current.GridID.x + 1 == last.GridID.x)
            {
                last.Open(MazeUnit.WEST);
                current.Open(MazeUnit.EAST);
            }

            if (current.GridID.y - 1 == last.GridID.y)
            {
                last.Open(MazeUnit.NORTH);
                current.Open(MazeUnit.SOUTH);
            }
            else if (current.GridID.y + 1 == last.GridID.y)
            {
                last.Open(MazeUnit.SOUTH);
                current.Open(MazeUnit.NORTH);
            }



            last = current;
        }
    }

    private void TryDisconnectingCurrentSelection()
    {
        if (currentSelection == null)
            return;

        if (!currentSelection.Any())
            return;


        if (currentSelection.Count == 1)
        {
            var unit = currentSelection.First().GetComponent<MazeUnit>();
            unit.Close(MazeUnit.NORTH);
            unit.Close(MazeUnit.SOUTH);
            unit.Close(MazeUnit.WEST);
            unit.Close(MazeUnit.EAST);
        }

        var iterator = currentSelection.GetEnumerator();

        MazeUnit last = null;

        while (iterator.MoveNext())
        {
            var current = iterator.Current.GetComponent<MazeUnit>();

            if (!last)
            {
                last = current;
                continue;
            }

            if (current.GridID.x - 1 == last.GridID.x)
            {
                last.Close(MazeUnit.EAST);
                current.Close(MazeUnit.WEST);
            }
            else if (current.GridID.x + 1 == last.GridID.x)
            {
                last.Close(MazeUnit.WEST);
                current.Close(MazeUnit.EAST);
            }

            if (current.GridID.y - 1 == last.GridID.y)
            {
                last.Close(MazeUnit.NORTH);
                current.Close(MazeUnit.SOUTH);
            }
            else if (current.GridID.y + 1 == last.GridID.y)
            {
                last.Close(MazeUnit.SOUTH);
                current.Close(MazeUnit.NORTH);
            }

            last = current;
        }
    }

    private void SetRandomObject()
    {
        var folder = string.Format("Assets/{0}", ObjectFolderName);
         
        if (AssetDatabase.IsValidFolder(folder))
        {
            var files = Directory.GetFiles(folder);
            foreach (var item in files)
            {
                Debug.Log(item);
            }
            //var assetImporter = ModelImporter.GetAtPath(fileName)
        }
        else
        {
            Debug.Log("Object folder not valid");
        }
    }

    #endregion

    protected override void RenderEditorGizmos()
    {
        Gizmos.color = Color.blue;

        if (currentSelection != null) { 
            foreach (var item in currentSelection)
            {
                Gizmos.DrawCube(item.transform.position + new Vector3(0, maze.RoomHigthInMeter / 2, 0), new Vector3(maze.RoomDimension.x, maze.RoomHigthInMeter, maze.RoomDimension.z));    
            }
        }

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

    PathInMaze pathShouldBeRemoved;
    string currentPathName = string.Empty;

    public override void RenderSceneViewUI()
    {  
        Handles.BeginGUI();

        GUILayout.BeginVertical(GUILayout.Width(200f));

        GUILayout.Label("Position in local Space of the maze");
        GUILayout.Label(string.Format("{0} {1} {2}", this.mouseHitPos.x, this.mouseHitPos.y, this.mouseHitPos.z));
        GUILayout.Label(string.Format("Marker: {0} {1} {2}", MarkerPosition.x, MarkerPosition.y, MarkerPosition.z));

        GUILayout.Space(10f);
        
        EditingModeEnabled = GUILayout.Toggle(EditingModeEnabled, "Editing Mode");

        #region Editing Mode UI

        if (EditingModeEnabled)
        {
            if (ActiveMode != MazeEditorMode.EDITING) {
                DisableModesExcept(MazeEditorMode.EDITING);
                EditorModeProcessEvent = null;
                EditorModeProcessEvent += EditingMode;
                ActiveMode = MazeEditorMode.EDITING;
            }
            GUILayout.BeginHorizontal();
            GUILayout.Space(15f);
            GUILayout.BeginVertical();
            modeAddEnabled = GUILayout.Toggle(!modeRemoveEnabled, "Adding Cells");
            modeRemoveEnabled = GUILayout.Toggle(!modeAddEnabled, "Erasing Cells");
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
        }
        else
        {
            modeRemoveEnabled = false;
            modeAddEnabled = false;
            EditorModeProcessEvent -= EditingMode;
        }
        #endregion

        GUILayout.Space(10f);

        SelectionModeEnabled = GUILayout.Toggle(SelectionModeEnabled, "Selection Mode");

        #region Selection Mode UI
        if (SelectionModeEnabled) {

            if (ActiveMode != MazeEditorMode.SELECTION) {
                DisableModesExcept(MazeEditorMode.SELECTION);
                
                currentSelection = new HashSet<GameObject>();
            
                EditorModeProcessEvent = null;
                EditorModeProcessEvent += SelectionMode;

                ActiveMode = MazeEditorMode.SELECTION;
            }

            if (GUILayout.Button("Connect", GUILayout.Width(100f)))
            {
                TryConnectingCurrentSelection();
            }

            if (GUILayout.Button("Disconnect", GUILayout.Width(100f)))
            {
                TryDisconnectingCurrentSelection();
            }

            if (GUILayout.Button("Set Random Object", GUILayout.Width(100f)))
            {
                SetRandomObject();
            }

        }
        else
        {
            EditorModeProcessEvent -= SelectionMode; 
            
            if(currentSelection != null)
                currentSelection.Clear();
        }
        #endregion

        GUILayout.Space(10f);

        GUILayout.EndVertical();
        
        Handles.EndGUI();
    }

    private void DisableModesExcept(MazeEditorMode mode)
    {
        switch (mode)
        {
            case MazeEditorMode.NONE:        
                EditingModeEnabled = false;
                SelectionModeEnabled = false;
                break;
            case MazeEditorMode.EDITING:
                SelectionModeEnabled = false;
                break;
            case MazeEditorMode.SELECTION:
                EditingModeEnabled = false;
                break;
            default:
                break;
        }
    }

    private void renderEmptyMazeGUI()
    {
        GUILayout.BeginVertical();

        if (GUILayout.Button("Edit selected maze", GUILayout.Width(255)))
        {
             
        } 

        GUILayout.EndVertical();
    }

}


public abstract class AMazeEditor : Editor {

    protected beMobileMaze maze;
    protected Action<Event> EditorModeProcessEvent;

    protected Vector3 MarkerPosition;

    protected Vector3 draggingStart;
    protected Vector2 currentTilePosition;
    protected Vector3 mouseHitPos;

    protected void TileHighlightingOnMouseCursor()
    {
        // if UpdateHitPosition return true we should update the scene views so that the marker will update in real time
        if (this.UpdateHitPosition())
        {
            this.RecalculateMarkerPosition();

            currentTilePosition = this.GetTilePositionFromMouseLocation();

            SceneView.currentDrawingSceneView.Repaint();
        }

    }

    protected void RenderTileHighlighting()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(MarkerPosition + new Vector3(0, maze.RoomHigthInMeter / 2, 0), new Vector3(maze.RoomDimension.x, maze.RoomHigthInMeter, maze.RoomDimension.z) * 1.1f);
    }

    public abstract void RenderSceneViewUI();

    public void OnSceneGUI()
    {
        TileHighlightingOnMouseCursor();

        RenderSceneViewUI();

        if (EditorModeProcessEvent != null)
            EditorModeProcessEvent(Event.current);
    }

    protected abstract void RenderEditorGizmos();

    #region General calculations based on tile editor

    /// <summary>
    /// Calculates the location in tile coordinates (Column/Row) of the mouse position
    /// </summary>
    /// <returns>Returns a <see cref="Vector2"/> type representing the Column and Row where the mouse of positioned over.</returns>
    protected Vector2 GetTilePositionFromMouseLocation()
    {
        // calculate column and row location from mouse hit location
        var pos = new Vector3(this.mouseHitPos.x / maze.RoomDimension.x, this.mouseHitPos.y / maze.transform.position.y, this.mouseHitPos.z / maze.RoomDimension.z);

        // round the numbers to the nearest whole number using 5 decimal place precision
        pos = new Vector3((int)Math.Round(pos.x, 5, MidpointRounding.ToEven), (int)Math.Round(pos.y, 5, MidpointRounding.ToEven), (int)Math.Round(pos.z, 5, MidpointRounding.ToEven));
        // do a check to ensure that the row and column are with the bounds of the tile mazeHost
        var col = (int)pos.x;
        var row = (int)pos.z;
        if (row < 0)
        {
            row = 0;
        }

        if (row > maze.Rows - 1)
        {
            row = maze.Rows - 1;
        }

        if (col < 0)
        {
            col = 0;
        }

        if (col > maze.Columns - 1)
        {
            col = maze.Columns - 1;
        }

        // return the column and row values
        return new Vector2(col, row);
    }

    /// <summary>
    /// Recalculates the position of the marker based on the location of the mouse pointer.
    /// </summary>
    protected void RecalculateMarkerPosition()
    {
        // store the tile position in world space
        var pos = new Vector3(currentTilePosition.x * maze.RoomDimension.x, 0, currentTilePosition.y * maze.RoomDimension.z);

        // set the TileMap.MarkerPosition value
        MarkerPosition = maze.transform.position + new Vector3(pos.x + (maze.RoomDimension.x / 2), pos.y, pos.z + (maze.RoomDimension.z / 2));
    }

    /// <summary>
    /// Calculates the position of the mouse over the tile mazeHost in local space coordinates.
    /// </summary>
    /// <returns>Returns true if the mouse is over the tile mazeHost.</returns>
    protected bool UpdateHitPosition()
    {
        // build a plane object that 
        var p = new Plane(maze.transform.TransformDirection(maze.transform.up), maze.transform.position);

        // build a ray type from the current mouse position
        var ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);

        // stores the hit location
        var hit = new Vector3();

        // stores the distance to the hit location
        float dist;

        // cast a ray to determine what location it intersects with the plane
        if (p.Raycast(ray, out dist))
        {
            // the ray hits the plane so we calculate the hit location in world space
            hit = ray.origin + (ray.direction.normalized * dist);
        }

        // convert the hit location from world space to local space
        var value = maze.transform.InverseTransformPoint(hit);

        // if the value is different then the current mouse hit location set the 
        // new mouse hit location and return true indicating a successful hit test
        if (value != this.mouseHitPos)
        {
            this.mouseHitPos = value;
            return true;
        }

        // return false if the hit test failed
        return false;
    }

    #endregion

}