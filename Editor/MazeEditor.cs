using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.Linq;

public enum MazeEditorMode {NONE, EDITING, SELECTION, PATH_CREATION}

[CustomEditor(typeof(beMobileMaze))]
public class MazeEditor : Editor
{
    private const string unitPrefabName = "MazeUnit";

    private beMobileMaze focusedMaze;
    public string UnitNamePattern = "Unit_{0}_{1}";
    public float unitFloorOffset = 0f;
     
    public Vector3 MarkerPosition;

    private HashSet<GameObject> currentSelection;
    private LinkedList<MazeUnit> pathInSelection;
    private string NameOfCurrentPath = String.Empty;

    private bool SelectionModeEnabled = false;
    private bool PathCreationEnabled = false;

    private bool EditingModeEnabled = false;
    private bool modeAddEnabled = false;
    private bool modeRemoveEnabled = false;

    private UnityEngine.Object referenceToPrefab;

    private string PathToMazePrefab = string.Empty;
    private string PathToMazeCompanionFolder = string.Empty;

    private MazeEditorMode ActiveMode = MazeEditorMode.NONE;

    private Vector3 draggingStart;
    private Vector2 currentTilePosition;
    private Vector3 mouseHitPos;

    private MazeUnit lastAddedUnit;

    private GUIStyle sceneViewEditorStyle;

    Action<Event> EditorModeProcessEvent;

    public void OnEnable()
    { 
        focusedMaze = (beMobileMaze)target;

        referenceToPrefab = PrefabUtility.GetPrefabParent(focusedMaze.gameObject);

        if (referenceToPrefab != null) { 
            PathToMazePrefab = AssetDatabase.GetAssetPath(referenceToPrefab);

            PathToMazeCompanionFolder = AssetHelper.GetOrCreateCompanionFolderForPrefab(PathToMazePrefab);
        }

        sceneViewEditorStyle = new GUIStyle();

        if(focusedMaze)
            focusedMaze.EditorGizmoCallbacks += RenderEditorGizmos;
    }

    public void OnDisable()
    {

        if (focusedMaze)
            focusedMaze.EditorGizmoCallbacks -= RenderEditorGizmos;
    }

    protected override void OnHeaderGUI()
    {
        //base.OnHeaderGUI();
    }

    public override void OnInspectorGUI()
    {
        if (focusedMaze == null) {
            renderEmptyMazeGUI();
        }

        GUILayout.BeginVertical();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Length of Maze");
        focusedMaze.MazeLengthInMeter = EditorGUILayout.FloatField(focusedMaze.MazeLengthInMeter, GUILayout.Width(50));
        GUILayout.Label("m");
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Width of Maze");
        focusedMaze.MazeWidthInMeter = EditorGUILayout.FloatField(focusedMaze.MazeWidthInMeter, GUILayout.Width(50));
        GUILayout.Label("m");
        GUILayout.EndHorizontal();

        focusedMaze.RoomDimension = EditorGUILayout.Vector3Field("Room Dimension", focusedMaze.RoomDimension); 

        GUILayout.BeginHorizontal();
        GUILayout.Label("Height of Rooms");
        focusedMaze.RoomHigthInMeter = EditorGUILayout.FloatField(focusedMaze.RoomHigthInMeter, GUILayout.Width(50));
        GUILayout.Label("m");
        GUILayout.EndHorizontal();
        
        GUILayout.BeginHorizontal(); 
        UnitNamePattern = EditorGUILayout.TextField("Unit Name Pattern", UnitNamePattern);
        GUILayout.EndHorizontal();

        GUILayout.EndVertical();

        GUILayout.BeginVertical();

        if (GUILayout.Button("Open Maze Editor", GUILayout.Width(255)))
        {
            MazeEditorWindow window = (MazeEditorWindow)EditorWindow.GetWindow(typeof(MazeEditorWindow));
            window.Init(focusedMaze);
        }

        if (GUILayout.Button("Clone Maze", GUILayout.Width(255)))
        {
            var clone = GameObject.Instantiate(focusedMaze);
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

        GUILayout.EndVertical();

    }

    private void UpdatePrefabOfCurrentMaze()
    {
       referenceToPrefab = PrefabUtility.ReplacePrefab(focusedMaze.gameObject, referenceToPrefab, ReplacePrefabOptions.ConnectToPrefab);
    }

    private void SavePrefabAndCreateCompanionFolder()
    {
        PathToMazePrefab = EditorUtility.SaveFilePanelInProject("Save maze", "maze.prefab", "prefab", "Save maze as Prefab");
        Debug.Log("Saved to " + PathToMazePrefab);
        referenceToPrefab = PrefabUtility.CreatePrefab(PathToMazePrefab, focusedMaze.gameObject, ReplacePrefabOptions.ConnectToPrefab);

        PathToMazeCompanionFolder = AssetHelper.GetOrCreateCompanionFolderForPrefab(PathToMazePrefab);

        Debug.Log("Create companion folder " + PathToMazePrefab);
    }

    private void OnSceneGUI()
    {
        // if UpdateHitPosition return true we should update the scene views so that the marker will update in real time
        if (this.UpdateHitPosition())
        { 
            this.RecalculateMarkerPosition();

            currentTilePosition = this.GetTilePositionFromMouseLocation();
            
            SceneView.currentDrawingSceneView.Repaint();
        }

        RenderSceneViewGUI();

        var _ce = Event.current;

        if(EditorModeProcessEvent != null)
            EditorModeProcessEvent(_ce);
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
        var unitHost = GameObject.Find(string.Format(UnitNamePattern, currentTilePosition.x, currentTilePosition.y));

        // if there is already a tile present and it is not a child of the game object we can just exit.
        if (unitHost != null && unitHost.transform.parent != focusedMaze.transform)
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

        var unit = this.CreateUnit(focusedMaze, currentTilePosition, unitHost);

        focusedMaze.Units.Add(unit);

    }

    private MazeUnit CreateUnit(beMobileMaze mazeHost, Vector2 tilePos, GameObject unit)
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
        unit.name = string.Format(UnitNamePattern, tilePos.x, tilePos.y);

        MazeUnit mazeUnit = unit.GetComponent<MazeUnit>();

        mazeUnit.Initialize(tilePos);

        return mazeUnit;
    }

    /// <summary>
    /// Erases a block at the pre-calculated mouse hit position
    /// </summary>
    private void Erase()
    {
        var unitHost = GameObject.Find(string.Format(UnitNamePattern, currentTilePosition.x, currentTilePosition.y));

        if (!unitHost)
        {
            Debug.Log("Nothing to erase!");
            return;
        }

        var unit = unitHost.GetComponent<MazeUnit>();

        // if a game object was found with the same assetName and it is a child we just destroy it immediately
        if (unit != null && unit.transform.parent == focusedMaze.transform)
        {
            focusedMaze.Units.Remove(unit);
            DestroyImmediate(unit.gameObject);
        }
    }

    private void SelectionMode(Event _ce)
    {
        int controlId = GUIUtility.GetControlID(FocusType.Passive);

        if (_ce.type == EventType.MouseDown || _ce.type == EventType.MouseDrag) {

            var unitHost = GameObject.Find(string.Format(UnitNamePattern, currentTilePosition.x, currentTilePosition.y));

            if (unitHost)
            {
                if(currentSelection.Contains(unitHost)){
                    currentSelection.Remove(unitHost);
                }
                else { 
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

    private void PathCreationMode(Event _ce)
    {
        int controlId = GUIUtility.GetControlID(FocusType.Passive);

        if (_ce.type == EventType.MouseDown || _ce.type == EventType.MouseDrag)
        {
            var unitHost = GameObject.Find(string.Format(UnitNamePattern, currentTilePosition.x, currentTilePosition.y));
           
            if (unitHost != null)
            {
                var unit = unitHost.GetComponent<MazeUnit>();

                if (unit)
                {
                    Debug.Log(string.Format("add {0} to path", unit.name));
                    pathInSelection.AddLast(unit);
                }

                if (unit && _ce.shift && pathInSelection.Any())
                {
                    pathInSelection.Remove(unit);
                }
            }
            else {
                Debug.Log("no element added");
            }

            

            GUIUtility.hotControl = controlId;
            _ce.Use();
        }
    }

    #endregion

    private void RenderEditorGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(MarkerPosition + new Vector3(0, focusedMaze.RoomHigthInMeter / 2, 0), new Vector3(focusedMaze.RoomDimension.x, focusedMaze.RoomHigthInMeter, focusedMaze.RoomDimension.z) * 1.1f);

        Gizmos.color = Color.blue;

        if (currentSelection != null) { 
            foreach (var item in currentSelection)
            {
                Gizmos.DrawCube(item.transform.position + new Vector3(0, focusedMaze.RoomHigthInMeter / 2, 0), new Vector3(focusedMaze.RoomDimension.x, focusedMaze.RoomHigthInMeter, focusedMaze.RoomDimension.z));    
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

                var hoveringDistance = new Vector3(0f, focusedMaze.RoomHigthInMeter, 0f);

                Gizmos.DrawLine(last.transform.position + hoveringDistance, iterator.Current.transform.position + hoveringDistance);

                last = iterator.Current;
            }
        }
    }

    PathInMaze pathShouldBeRemoved;
    string currentPathName = string.Empty;

    private void RenderSceneViewGUI()
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

                currentSelection = new HashSet<GameObject>();
            
                EditorModeProcessEvent = null;
                EditorModeProcessEvent += SelectionMode;

                ActiveMode = MazeEditorMode.SELECTION;
            }

            if (GUILayout.Button("Connect"))
            {
                TryConnectingCurrentSelection();
            }

            if (GUILayout.Button("Disconnect"))
            {
                TryDisconnectingCurrentSelection();
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

        #region Path creation mode

        PathCreationEnabled = GUILayout.Toggle(PathCreationEnabled, "Path creation");

        if (PathCreationEnabled) {
            
            if(ActiveMode != MazeEditorMode.PATH_CREATION){
            
                    pathInSelection = new LinkedList<MazeUnit>();

                    EditorModeProcessEvent += PathCreationMode;
                    ActiveMode = MazeEditorMode.PATH_CREATION;
                }

            currentPathName = GUILayout.TextField(currentPathName, GUILayout.Width(80f));

            if (currentPathName != string.Empty && PathIsValid(pathInSelection) && GUILayout.Button("Save Path", GUILayout.Width(75f)))
            {
                var pathInMaze = ScriptableObject.CreateInstance<PathInMaze>();
                
                pathInMaze.PathName = currentPathName;

                pathInMaze.Setup(pathInSelection);

                if (PathToMazePrefab == string.Empty && PathToMazeCompanionFolder == string.Empty)
                {
                    SavePrefabAndCreateCompanionFolder();
                }

                if (!AssetDatabase.IsValidFolder(PathToMazeCompanionFolder))
                {
                    PathToMazeCompanionFolder = AssetHelper.GetOrCreateCompanionFolderForPrefab(PathToMazePrefab);
                }

                AssetDatabase.CreateAsset(pathInMaze, string.Format("{0}/{1}", PathToMazeCompanionFolder, currentPathName));

                focusedMaze.Paths.Add(pathInMaze);

                PrefabUtility.ReplacePrefab(focusedMaze.gameObject, referenceToPrefab, ReplacePrefabOptions.ConnectToPrefab);
            }

            if (focusedMaze.Paths.Any()) { 

                GUILayout.Space(4f);

                GUILayout.Label("Existing Paths");
            
                GUILayout.Space(2f);

                foreach (var path in focusedMaze.Paths)
                {
                    GUILayout.BeginHorizontal(GUILayout.Width(100f));
                
                        if (GUILayout.Button(path.name))
                        {
                            pathInSelection = focusedMaze.CreatePathFromGridIDs(path.GridIDs);
                        }

                        if (GUILayout.Button("X", GUILayout.Width(20f)))
                        {
                            pathShouldBeRemoved = path;
                        }

                    GUILayout.EndHorizontal();
                }

                if (pathShouldBeRemoved != null) { 
                    focusedMaze.Paths.Remove(pathShouldBeRemoved);
                    pathShouldBeRemoved = null;
                }
            }
        }
        else
        {
            EditorModeProcessEvent -= PathCreationMode;

            if(pathInSelection != null)
                pathInSelection.Clear();

        }
        #endregion

        GUILayout.Space(10f);
        
        GUILayout.EndVertical();
        
        Handles.EndGUI();
    }

    private bool PathIsValid(LinkedList<MazeUnit> path)
    {
        if (path == null)
            return false;

        bool hasEnoughElements = path.Count > 1;

        return hasEnoughElements;
    }

    private void renderEmptyMazeGUI()
    {
        GUILayout.BeginVertical();

        if (GUILayout.Button("Edit selected maze", GUILayout.Width(255)))
        {
             
        } 

        GUILayout.EndVertical();
    }

    #region General calculations based on tile editor 

    /// <summary>
    /// Calculates the location in tile coordinates (Column/Row) of the mouse position
    /// </summary>
    /// <returns>Returns a <see cref="Vector2"/> type representing the Column and Row where the mouse of positioned over.</returns>
    private Vector2 GetTilePositionFromMouseLocation()
    {
        // calculate column and row location from mouse hit location
        var pos = new Vector3(this.mouseHitPos.x / focusedMaze.RoomDimension.x, this.mouseHitPos.y / focusedMaze.transform.position.y, this.mouseHitPos.z / focusedMaze.RoomDimension.z);

        // round the numbers to the nearest whole number using 5 decimal place precision
        pos = new Vector3((int)Math.Round(pos.x, 5, MidpointRounding.ToEven), (int)Math.Round(pos.y, 5, MidpointRounding.ToEven), (int)Math.Round(pos.z, 5, MidpointRounding.ToEven));
        // do a check to ensure that the row and column are with the bounds of the tile mazeHost
        var col = (int)pos.x;
        var row = (int)pos.z;
        if (row < 0)
        {
            row = 0;
        }

        if (row > focusedMaze.Rows - 1)
        {
            row = focusedMaze.Rows - 1;
        }

        if (col < 0)
        {
            col = 0;
        }

        if (col > focusedMaze.Columns - 1)
        {
            col = focusedMaze.Columns - 1;
        }

        // return the column and row values
        return new Vector2(col, row);
    }

    /// <summary>
    /// Recalculates the position of the marker based on the location of the mouse pointer.
    /// </summary>
    private void RecalculateMarkerPosition()
    {  
        // store the tile position in world space
        var pos = new Vector3(currentTilePosition.x * focusedMaze.RoomDimension.x, 0, currentTilePosition.y * focusedMaze.RoomDimension.z);
        
        // set the TileMap.MarkerPosition value
        MarkerPosition = focusedMaze.transform.position + new Vector3(pos.x + (focusedMaze.RoomDimension.x / 2), pos.y, pos.z + (focusedMaze.RoomDimension.z / 2));
    }

    /// <summary>
    /// Calculates the position of the mouse over the tile mazeHost in local space coordinates.
    /// </summary>
    /// <returns>Returns true if the mouse is over the tile mazeHost.</returns>
    private bool UpdateHitPosition()
    {
        // build a plane object that 
        var p = new Plane(focusedMaze.transform.TransformDirection(focusedMaze.transform.up), focusedMaze.transform.position);
        
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
        var value = focusedMaze.transform.InverseTransformPoint(hit);

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