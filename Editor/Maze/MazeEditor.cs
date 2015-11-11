using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;

public enum MazeEditorMode { NONE, EDITING, SELECTION }

[CustomEditor(typeof(beMobileMaze))]
public class MazeEditor : AMazeEditor
{
    public const string STD_UNIT_PREFAB_NAME = "MazeUnit";
    
    public Material FloorMaterial;
    public Material WallMaterial;
    public Material TopMaterial;
     
    private HashSet<GameObject> currentSelection;

    public float unitFloorOffset = 0f;

    private string PathToMazePrefab = string.Empty;

    private bool SelectionModeEnabled = false;

    private bool EditingModeEnabled = false;
    private bool modeAddEnabled = false;
    private bool modeRemoveEnabled = false;

    private UnityEngine.Object referenceToPrefab;

    private MazeEditorMode ActiveMode = MazeEditorMode.NONE;

    private MazeUnit lastAddedUnit;
     
    public void OnEnable()
    { 
        maze = (beMobileMaze)target;

        if(maze.Grid == null)
        {
            MazeEditorUtil.RebuildGrid(maze);
        }

        referenceToPrefab = PrefabUtility.GetPrefabParent(maze.gameObject);

        if (referenceToPrefab != null) { 
            PathToMazePrefab = AssetDatabase.GetAssetPath(referenceToPrefab);
        }

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

    public override void OnInspectorGUI()
    {
        if (maze == null) {
            renderEmptyMazeGUI();
        }
        
        GUILayout.BeginVertical();

        GUILayout.Label("Properties", EditorStyles.boldLabel);
        
        EditorGUILayout.LabelField("Length of Maze (m)", maze.MazeLengthInMeter.ToString());
 
        EditorGUILayout.LabelField("Width of Maze (m)", maze.MazeWidthInMeter.ToString());

        EditorGUILayout.LabelField("Units:", maze.Units.Count.ToString());

        EditorGUILayout.LabelField("Room size (m):", maze.RoomDimension.ToString());

        EditorGUILayout.Space();

        if (GUILayout.Button("Open Customizer", GUILayout.Height(40)))
        {
            var window = CreateInstance<MazeCustomizer>();

            window.Initialize(maze);

            window.Show();
        }

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Close Maze Roof"))
        {
            foreach (var unit in maze.Units)
            {
                var topTransform = unit.transform.FindChild("Top");
                if (topTransform != null)
                    topTransform.gameObject.SetActive(true);
            }
        }

        if (GUILayout.Button("Open Maze Roof"))
        {
            foreach (var unit in maze.Units)
            {
                var topTransform = unit.transform.FindChild("Top");
                if (topTransform != null)
                    topTransform.gameObject.SetActive(false);
            }
        }

        EditorGUILayout.EndHorizontal();

        if (GUILayout.Button("Search for Units")) 
        {
            MazeEditorUtil.CacheUnitsIn(maze);
        }

        if (GUILayout.Button("Configure Grid"))
        {
            MazeEditorUtil.RebuildGrid(maze);
        }

        if (referenceToPrefab && GUILayout.Button("Update Prefab"))
        {
            UpdatePrefabOfCurrentMaze();
        }

        if (GUILayout.Button("Repair Unit List"))
        {
            var unitsExceptMissingReference = maze.Units.Where(u => u != null);
            
            maze.Units.Clear();
            
            foreach (var item in unitsExceptMissingReference)
            {
                maze.Units.Add(item);
            }
            
            for (int i = 0; i < maze.transform.childCount; i++)
            {
                var existing = maze.transform.GetChild(i).GetComponent<MazeUnit>();

                if (existing != null && !maze.Units.Contains(existing))
                    maze.Units.Add(existing);
            }

        }

        GUILayout.EndVertical();

    }

    private void ApplyToAllMazeUnits(Action<MazeUnit> action)
    {
        foreach (var item in maze.Units)
        {
            action(item);
        }
    }
    

    private void UpdatePrefabOfCurrentMaze()
    {
       referenceToPrefab = PrefabUtility.ReplacePrefab(maze.gameObject, referenceToPrefab, ReplacePrefabOptions.ConnectToPrefab);

       EditorUtility.SetDirty(referenceToPrefab);
       EditorApplication.delayCall += AssetDatabase.SaveAssets;
    }

    private void SavePrefabAndCreateCompanionFolder()
    {
        PathToMazePrefab = EditorUtility.SaveFilePanelInProject("Save maze", "maze.prefab", "prefab", "Save maze as Prefab");
        Debug.Log("Saved to " + PathToMazePrefab);
        referenceToPrefab = PrefabUtility.CreatePrefab(PathToMazePrefab, maze.gameObject, ReplacePrefabOptions.ConnectToPrefab);
        
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
            string mazeUnitPrefab = EditorEnvironmentConstants.Get_BASE_ASSET_PATH() + "/" + EditorEnvironmentConstants.PREFABS_DIR + "/" + STD_UNIT_PREFAB_NAME + EditorEnvironmentConstants.PREFAB_EXTENSION;

            var obj = AssetDatabase.LoadAssetAtPath<GameObject>(mazeUnitPrefab);
             
            if (obj)
            {
                unitHost = PrefabUtility.InstantiatePrefab(obj) as GameObject;
                PrefabUtility.DisconnectPrefabInstance(unitHost);
            }
            else
            {
                Debug.LogError(string.Format("\"{0}\" not found!", mazeUnitPrefab));
                return;
            }
        }

        var unit = MazeEditorUtil.InitializeUnit(maze, currentTilePosition, unitFloorOffset,unitHost);
        maze.Grid[(int)currentTilePosition.x, (int)currentTilePosition.y] = unit;
        maze.Units.Add(unit);

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
            Debug.Log(current.GridID.ToString());
            // check if current and last are really neighbors:
            if (Math.Abs(current.GridID.x - last.GridID.x) + Math.Abs(current.GridID.y - last.GridID.y) == 1)
            {
                // check which direction we go, possibilities:
                if (current.GridID.x - last.GridID.x == 1) // going east
                {
                    last.Open(OpenDirections.East);
                    current.Open(OpenDirections.West);
                }
                else if (current.GridID.x - last.GridID.x == -1) // going west
                {
                    last.Open(OpenDirections.West);
                    current.Open(OpenDirections.East);
                }

                if (current.GridID.y - last.GridID.y == 1) // going north
                {
                    last.Open(OpenDirections.North);
                    current.Open(OpenDirections.South);
                }
                else if (current.GridID.y - last.GridID.y == -1) // going south
                {
                    last.Open(OpenDirections.South);
                    current.Open(OpenDirections.North);
                }
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
            unit.Close(OpenDirections.North);
            unit.Close(OpenDirections.South);
            unit.Close(OpenDirections.West);
            unit.Close(OpenDirections.East);
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
                last.Close(OpenDirections.East);
                current.Close(OpenDirections.West);
            }
            else if (current.GridID.x + 1 == last.GridID.x)
            {
                last.Close(OpenDirections.West);
                current.Close(OpenDirections.East);
            }

            if (current.GridID.y - 1 == last.GridID.y)
            {
                last.Close(OpenDirections.North);
                current.Close(OpenDirections.South);
            }
            else if (current.GridID.y + 1 == last.GridID.y)
            {
                last.Close(OpenDirections.South);
                current.Close(OpenDirections.North);
            }

            last = current;
        }
    }

    #endregion

    protected override void RenderEditorGizmos()
    {
        var tempMatrix = Gizmos.matrix;

        Gizmos.matrix = maze.transform.localToWorldMatrix;

        var temp = Handles.matrix;
        Handles.matrix = Gizmos.matrix;

        drawFloorGrid();

        Gizmos.color = Color.blue;

        if (currentSelection != null) { 
            foreach (var item in currentSelection)
            {
                var pos = item.transform.localPosition + new Vector3(0, maze.RoomDimension.y / 2, 0);
                Gizmos.DrawCube(pos, new Vector3(maze.RoomDimension.x, maze.RoomDimension.y, maze.RoomDimension.z));    
            }
        }

        Handles.matrix = temp;
        Gizmos.matrix = tempMatrix;
    }

    private void drawFloorGrid()
    {
        // store map width, height and position
        var mapWidth = maze.MazeWidthInMeter;
        var mapHeight = maze.MazeLengthInMeter;

        //var position = maze.transform.position;
        var position = new Vector3(0, 0, 0);

        // draw layer border
        Gizmos.color = Color.white;
        Gizmos.DrawLine(position, position + new Vector3(mapWidth, 0, 0));
        Gizmos.DrawLine(position, position + new Vector3(0, 0, mapHeight));
        Gizmos.DrawLine(position + new Vector3(mapWidth, 0, 0), position + new Vector3(mapWidth, 0, mapHeight));
        Gizmos.DrawLine(position + new Vector3(0, 0, mapHeight), position + new Vector3(mapWidth, 0, mapHeight));
        
        Vector3 lineStart;
        Vector3 lineEnde;
        // draw tile cells
        Gizmos.color = Color.green;

        for (float i = 1; i <= maze.Columns; i++)
        {
            float xOffset = i * maze.RoomDimension.x;

            lineStart = position + new Vector3(xOffset, 0, 0);
            lineEnde = position + new Vector3(xOffset, 0, mapHeight);

            var labelPos = position + new Vector3(xOffset, 0, 0);
            Handles.Label(labelPos, i.ToString());

            Gizmos.DrawLine(lineStart, lineEnde);
        }

        for (float i = 1; i <= maze.Rows; i++)
        {
            float yoffset = i * maze.RoomDimension.z;

            lineStart = position + new Vector3(0, 0, yoffset);
            lineEnde = position + new Vector3(mapWidth, 0, yoffset);
            
            var labelPos = position + new Vector3(0, 0, yoffset);
            Handles.Label(labelPos, i.ToString());

            Gizmos.DrawLine(lineStart, lineEnde);
        }

        var zeroField = new Vector3(position.x + (maze.RoomDimension.x / 2), unitFloorOffset, position.x + (maze.RoomDimension.x / 2));

        Gizmos.color = new Color(Color.green.r, Color.green.g, Color.green.b, 0.1f);

        Gizmos.DrawCube(zeroField, new Vector3(maze.RoomDimension.x - 0.1f, 0, maze.RoomDimension.z - 0.1f));
    }

    public override void RenderSceneViewUI()
    {
        if (EditorApplication.isPlaying)
            return;


        Handles.BeginGUI();

        GUILayout.BeginVertical(GUILayout.Width(200f));

        GUILayout.Label("Position in local Space of the maze");
        GUILayout.Label(string.Format("{0} {1} {2}", this.mouseHitPos.x, this.mouseHitPos.y, this.mouseHitPos.z));
        GUILayout.Label(string.Format("Marker: {0} {1} {2}", MarkerPosition.x, MarkerPosition.y, MarkerPosition.z));

        GUILayout.Space(10f);
        
        #region Editing Mode UI

        if (MazeDoesNotContainPaths())
        { 
            EditingModeEnabled = GUILayout.Toggle(EditingModeEnabled, "Editing Mode");

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
        }
        else
        {
            GUILayout.Label("Editing disabled, \n Maze contains paths!");
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
        }
        else
        {
            EditorModeProcessEvent -= SelectionMode; 
            
            if(currentSelection != null)
                currentSelection.Clear();
        }
        #endregion

        GUILayout.Space(10f);

        GUILayout.Label("Grid:");
        GUILayout.Space(3f);

        RenderMazeGrid();

        GUILayout.EndVertical();
        
        Handles.EndGUI();
    }

    private bool MazeDoesNotContainPaths()
    {
        var controller = maze.GetComponent<PathController>();
        
        if (controller == null)
            return false;

        if (!controller.Paths.Any() || controller.Paths.Any((p) =>  p == null))
            controller.ForcePathLookup();

        var hasPaths = controller.Paths.Any(p => p.PathElements.Any());

        return !hasPaths;
    }

    void RenderMazeGrid() {

        if (maze.Grid == null)
            return;

        StringBuilder gridCode = new StringBuilder();
        StringBuilder line = new StringBuilder();

        int cols = maze.Grid.GetLength(0);
        int rows = maze.Grid.GetLength(1);

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                if (maze.Grid[c, r] != null)
                    line.AppendFormat(" {0}", 1);
                else
                    line.AppendFormat(" {0}", 0);
            }

            gridCode.AppendLine(line.ToString());
            line.Remove(0, line.Length);
        }

        GUILayout.Label(gridCode.ToString());
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
    protected Color MarkerColor = Color.blue;
    protected Vector3 draggingStart;
    protected Vector2 currentTilePosition;
    protected Vector3 mouseHitPos;

    protected GUIStyle sceneViewUIStyle;

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
        var tempMatrix = Gizmos.matrix;

        Gizmos.matrix = maze.transform.localToWorldMatrix;

        Gizmos.color = MarkerColor;
        
        var pos = MarkerPosition + new Vector3(0, maze.RoomDimension.y / 2, 0);
        
        Gizmos.DrawWireCube(pos, new Vector3(maze.RoomDimension.x, maze.RoomDimension.y, maze.RoomDimension.z) * 1.1f);

        var temp = Handles.matrix;
        
        Handles.matrix = Gizmos.matrix;

        Handles.Label(pos, string.Format("{0}.{1}", (int) MarkerPosition.x, (int) MarkerPosition.z), sceneViewUIStyle );

        Handles.matrix = temp;

        Gizmos.matrix = tempMatrix;
    }

    public abstract void RenderSceneViewUI();

    public void OnSceneGUI()
    {
        SetupGUIStyle();

        TileHighlightingOnMouseCursor();

        RenderSceneViewUI();

        if (EditorModeProcessEvent != null)
            EditorModeProcessEvent(Event.current);
    }

    protected virtual void SetupGUIStyle()
    {
        sceneViewUIStyle = new GUIStyle();
        sceneViewUIStyle.normal.textColor = Color.blue;
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
        // do a check to ensure that the row and column are with the bounds of the tile maze
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
        MarkerPosition = new Vector3(pos.x + (maze.RoomDimension.x / 2), pos.y, pos.z + (maze.RoomDimension.z / 2));
    }

    /// <summary>
    /// Calculates the position of the mouse over the tile maze in local space coordinates.
    /// </summary>
    /// <returns>Returns true if the mouse is over the tile maze.</returns>
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