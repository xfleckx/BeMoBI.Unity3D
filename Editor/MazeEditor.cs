using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;

[CustomEditor(typeof(beMobileMaze))]
public class MazeEditor : Editor
{
    private const string unitPrefabName = "MazeUnit";

    private beMobileMaze focusedMaze;
    public string UnitNamePattern = "Unit_{0}_{1}";
    public float unitFloorOffset = 0f;
     
    public Vector3 MarkerPosition;

    private List<GameObject> currentSelection;

    public bool SelectionModeEnabled = false;
    public bool EditingModeEnabled = false;
    public bool modeAddEnabled = false;
    public bool modeRemoveEnabled = false;

    private Vector3 draggingStart;
    private Vector2 currentTilePosition;
    private Vector3 mouseHitPos;

    private MazeUnit lastAddedUnit;

    private GUIStyle sceneViewEditorStyle;

    Action<Event> EditorModeProcessEvent;

    public void OnEnable()
    { 
        focusedMaze = (beMobileMaze)target;
        MazeSceneViewEditor.Enable();
        sceneViewEditorStyle = new GUIStyle();

        if(focusedMaze)
            focusedMaze.EditorGizmoCallbacks += RenderEditorGizmos;
    }

    public void OnDisable()
    {
        MazeSceneViewEditor.Disable();

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
            GameObject.Instantiate(focusedMaze);
        }

        if (GUILayout.Button("Create Maze Prefab"))
        {

        }

        GUILayout.EndVertical();

    }

    private void OnSceneGUI()
    {
        // if UpdateHitPosition return true we should update the scene views so that the marker will update in real time
        if (this.UpdateHitPosition())
        { 
            this.RecalculateMarkerPosition();

            currentTilePosition = this.GetTilePositionFromMouseLocation();

            focusedMaze.drawEditingHelper = true;
            
            SceneView.currentDrawingSceneView.Repaint();
        }

        RenderInfoGUI();

        var _ce = Event.current;

        if(EditorModeProcessEvent != null)
            EditorModeProcessEvent(_ce);
    }

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
            Debug.Log("MouseDown");
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

    private void SelectionMode(Event _ce)
    {
        int controlId = GUIUtility.GetControlID(FocusType.Passive);

        if (_ce.type == EventType.MouseDown || _ce.type == EventType.MouseDrag) {

            var unitHost = GameObject.Find(string.Format(UnitNamePattern, currentTilePosition.x, currentTilePosition.y));

            //! Don't use Selection... a manual selection process necessary
            //if (unitHost && _ce.shift)
            //{
            //    currentSelection = new List<GameObject>(Selection.gameObjects);
            //    currentSelection.Add(unitHost);
            //    Selection.objects = currentSelection.ToArray();
            //}
            //else if (unitHost)
            //{
            //    Selection.activeGameObject = unitHost;
            //}

            GUIUtility.hotControl = controlId;
            _ce.Use();
        }
        
    }


    private void RenderEditorGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(MarkerPosition + new Vector3(0, focusedMaze.RoomHigthInMeter / 2, 0), new Vector3(focusedMaze.RoomDimension.x, focusedMaze.RoomHigthInMeter, focusedMaze.RoomDimension.z) * 1.1f);
    }

    private void RenderInfoGUI()
    {  
        Handles.BeginGUI();

        GUILayout.BeginVertical();
        GUILayout.Space(10f);
        GUILayout.Label("Mouse position in local Space of the maze");
        GUILayout.Label(string.Format("{0} {1} {2}", this.mouseHitPos.x, this.mouseHitPos.y, this.mouseHitPos.z));
        GUILayout.Label(string.Format("Marker: {0} {1} {2}", MarkerPosition.x, MarkerPosition.y, MarkerPosition.z));

        GUILayout.Space(10f);
        
        EditingModeEnabled = GUILayout.Toggle(EditingModeEnabled, "Editing Mode");

        if (EditingModeEnabled)
        {
            EditorModeProcessEvent = null;
            EditorModeProcessEvent += EditingMode;

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

        GUILayout.Space(10f);

        SelectionModeEnabled = GUILayout.Toggle(SelectionModeEnabled, "Selection Mode");

        if (SelectionModeEnabled) { 
            EditorModeProcessEvent = null;
            EditorModeProcessEvent += SelectionMode;
        }
        else
        {
            EditorModeProcessEvent -= SelectionMode;
        }

        GUILayout.EndVertical();
        Handles.EndGUI();
    }

    private void renderEmptyMazeGUI()
    {
        GUILayout.BeginVertical();

        if (GUILayout.Button("Edit selected maze", GUILayout.Width(255)))
        {
             
        } 

        GUILayout.EndVertical();
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
            var obj = Resources.Load(unitPrefabName);

            if (obj){
                unitHost = (GameObject)GameObject.Instantiate(obj);
            }
            else { 
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

}