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

    private List<GameObject> currentSelection;

    public bool EditingModeEnabled = true;
    public bool modeAddEnabled = false;
    public bool modeRemoveEnabled = false;

    private Vector3 draggingStart;
     
    private Vector3 mouseHitPos;
    private MazeUnit lastAddedUnit;

    public void OnEnable()
    { 
        focusedMaze = (beMobileMaze)target;
        MazeSceneViewEditor.Enable();
    }

    public void OnDisable()
    {
        MazeSceneViewEditor.Disable();
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

        EditingModeEnabled = EditorGUILayout.BeginToggleGroup("Editing Mode", EditingModeEnabled);

        modeAddEnabled = EditorGUILayout.Toggle("Add", modeAddEnabled);

        if (modeAddEnabled)
            modeRemoveEnabled = false;

        modeRemoveEnabled = EditorGUILayout.Toggle("Remove", modeRemoveEnabled);

        if (modeRemoveEnabled)
            modeAddEnabled = false;  

        EditorGUILayout.EndToggleGroup();

        if (GUILayout.Button("Open Maze Editor", GUILayout.Width(255)))
        {
            MazeEditorWindow window = (MazeEditorWindow)EditorWindow.GetWindow(typeof(MazeEditorWindow));
            window.Init(focusedMaze);
        }

        if (GUILayout.Button("Clone Maze", GUILayout.Width(255)))
        {
            GameObject.Instantiate(focusedMaze);
        }
         
        GUILayout.EndVertical();

    }

    private void OnSceneGUI()
    {
        // if UpdateHitPosition return true we should update the scene views so that the marker will update in real time
        if (this.UpdateHitPosition())
        {
            SceneView.RepaintAll();
        }

        // get a reference to the current event
        Event current = Event.current;

        focusedMaze.drawEditingHelper = false;
        
        // if the mouse is positioned over the layer allow drawing actions to occur
        if (this.IsMouseOnLayer() && EditingModeEnabled)
        {
            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
            // Calculate the location of the marker based on the location of the mouse
            this.RecalculateMarkerPosition();

            focusedMaze.drawEditingHelper = true;

            // if mouse down or mouse drag event occurred
            if (current.type == EventType.MouseDown || current.type == EventType.MouseDrag)
            {
                if (current.button == 1)
                { 
                    if(modeAddEnabled)
                        this.Draw();
                    else if (modeRemoveEnabled)
                        this.Erase();

                    currentSelection = null;

                    Selection.activeObject = focusedMaze.gameObject;

                }
                else if (current.button == 0)
                {
                    Vector2 unitPosition = GetTilePositionFromMouseLocation();
                    
                    string selectionTargetName = String.Format(UnitNamePattern, unitPosition.x, unitPosition.y);
                    var existingUnit = GameObject.Find(selectionTargetName);

                    if (existingUnit && current.shift)
                    {
                        currentSelection = new List<GameObject>(Selection.gameObjects);
                        currentSelection.Add(existingUnit);
                        Selection.objects = currentSelection.ToArray();
                    }
                    else if(existingUnit)
                    {
                        Selection.activeGameObject = existingUnit;
                    }
                }
            } 
            else
            {
                Selection.activeObject = focusedMaze.gameObject;
            }

            current.Use();
        }

        // draw a UI tip in scene view informing user how to draw & erase tiles
        Handles.BeginGUI();
            GUILayout.BeginVertical();
            GUILayout.Label("Ctrl + LMB: Draw or Select");
            GUILayout.Label("Shit + RMB: Erase");
            GUILayout.Label(string.Format("Marker: {0} {1} {2}", focusedMaze.MarkerPosition.x, focusedMaze.MarkerPosition.y, focusedMaze.MarkerPosition.z));
            GUILayout.Label(string.Format("Marker: {0} {1} {2}", focusedMaze.MarkerPosition.x, focusedMaze.MarkerPosition.y, focusedMaze.MarkerPosition.z));
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
        // get reference to the TileMap component
        var map = (beMobileMaze)this.target;

        // Calculate the position of the mouse over the tile layer
        var tilePos = this.GetTilePositionFromMouseLocation();

        // Given the tile position check to see if a tile has already been created at that location
        var unitHost = GameObject.Find(string.Format(UnitNamePattern, tilePos.x, tilePos.y));

        // if there is already a tile present and it is not a child of the game object we can just exit.
        if (unitHost != null && unitHost.transform.parent != map.transform)
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
       
        var unit = this.CreateUnit(map, tilePos, unitHost);
       
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
        // get reference to the TileMap component
        var map = (beMobileMaze)this.target;

        // Calculate the position of the mouse over the tile layer
        var tilePos = this.GetTilePositionFromMouseLocation();

        // Given the tile position check to see if a tile has already been created at that location
        var unitHost = GameObject.Find(string.Format(UnitNamePattern, tilePos.x, tilePos.y));

        if (!unitHost)
        {
            Debug.Log("Nothing to erase!");
            return;
        }

        var unit = unitHost.GetComponent<MazeUnit>();

        // if a game object was found with the same assetName and it is a child we just destroy it immediately
        if (unit != null && unit.transform.parent == map.transform)
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
        // get reference to the tile mazeHost component
        var map = (beMobileMaze)this.target;

        // calculate column and row location from mouse hit location
        var pos = new Vector3(this.mouseHitPos.x / map.RoomDimension.x, this.mouseHitPos.y / map.transform.position.y, this.mouseHitPos.z / map.RoomDimension.z);

        // round the numbers to the nearest whole number using 5 decimal place precision
        pos = new Vector3((int)Math.Round(pos.x, 5, MidpointRounding.ToEven), (int)Math.Round(pos.y, 5, MidpointRounding.ToEven), (int)Math.Round(pos.z, 5, MidpointRounding.ToEven));
        // do a check to ensure that the row and column are with the bounds of the tile mazeHost
        var col = (int)pos.x;
        var row = (int)pos.z;
        if (row < 0)
        {
            row = 0;
        }

        if (row > map.Rows - 1)
        {
            row = map.Rows - 1;
        }

        if (col < 0)
        {
            col = 0;
        }

        if (col > map.Columns - 1)
        {
            col = map.Columns - 1;
        }

        // return the column and row values
        return new Vector2(col, row);
    }

    /// <summary>
    /// Returns true or false depending if the mouse is positioned over the tile mazeHost.
    /// </summary>
    /// <returns>Will return true if the mouse is positioned over the tile mazeHost.</returns>
    private bool IsMouseOnLayer()
    {
        // get reference to the tile mazeHost component
        var map = (beMobileMaze)this.target;

        // return true or false depending if the mouse is positioned over the mazeHost
        //return this.mouseHitPos.x > 0 && this.mouseHitPos.x < (map.Columns * map.RoomDimension.x) &&
        //       this.mouseHitPos.z > 0 && this.mouseHitPos.z < (map.Rows * map.RoomDimension.z);

        float mapDimX = map.Columns * map.RoomDimension.x;
        float mapDimZ = map.Rows * map.RoomDimension.y;

        bool isInXspace = this.mouseHitPos.x > map.transform.position.x && this.mouseHitPos.x < mapDimX;
        bool isInZspace = this.mouseHitPos.z > map.transform.position.z && this.mouseHitPos.z < mapDimZ;

        return isInXspace && isInZspace;
    }

    /// <summary>
    /// Recalculates the position of the marker based on the location of the mouse pointer.
    /// </summary>
    private void RecalculateMarkerPosition()
    {
        // get reference to the tile mazeHost component
        var map = (beMobileMaze)this.target;

        // store the tile location (Column/Row) based on the current location of the mouse pointer
        var tilepos = this.GetTilePositionFromMouseLocation();

        // store the tile position in world space
        var pos = new Vector3(tilepos.x * map.RoomDimension.x, 0, tilepos.y * map.RoomDimension.z);
        
        // set the TileMap.MarkerPosition value
        map.MarkerPosition = map.transform.position + new Vector3(pos.x + (map.RoomDimension.x / 2), pos.y, pos.z + (map.RoomDimension.z / 2));
    }

    /// <summary>
    /// Calculates the position of the mouse over the tile mazeHost in local space coordinates.
    /// </summary>
    /// <returns>Returns true if the mouse is over the tile mazeHost.</returns>
    private bool UpdateHitPosition()
    {
        // get reference to the tile mazeHost component
        var map = (beMobileMaze)this.target;

        // build a plane object that 
        var p = new Plane(map.transform.TransformDirection(map.transform.up), map.transform.position);
        
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
        var value = map.transform.InverseTransformPoint(hit);

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