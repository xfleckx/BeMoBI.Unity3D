using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class MazeEditorWindow : EditorWindow
{

    string MazeName = "beMoBI.Maze";
    string Description = "A example maze";
    bool CreateWaypoints = true;
    
    float WallThickness = 0.1f;
    float WallLength = 1.4f;
    Vector2 UnitDimension = new Vector2(1.5f,1.5f);
    float mazeWidth = 0;
    float mazeLength = 0;

    int unitRows = 0;
    int unitColumns = 0;

    private beMobileMaze selectedMaze;

    private GameObject wallPrototype;
    private Editor wallPrototypeEditor;

    private GameObject edgePrototype;
    private Editor edgePrototypeEditor;

    //private string UnitNamePattern = "{0}.{1}";

    private Vector2 scrollPos;

    private bool showWayPointOptions = true;

    public void Init() {
        GameObject host = new GameObject("A beMoBI maze");
        host.AddComponent(typeof(beMobileMaze));
        selectedMaze = host.GetComponent<beMobileMaze>();
    }

    internal void Init(beMobileMaze focusedMaze)
    {
        selectedMaze = focusedMaze; 
        title = "Maze Editor";
    }

    void OnGUI() 
    {
        EditorGUILayout.BeginVertical(GUILayout.Width(250));

        GUILayout.Label("Base Settings", EditorStyles.boldLabel);
        MazeName = EditorGUILayout.TextField("Name:", selectedMaze.name);
        Description = EditorGUILayout.TextField("Description:", Description);
        WallThickness = EditorGUILayout.Slider("Wall thickness", WallThickness, 0.01f, 0.9f);
        UnitDimension = EditorGUILayout.Vector2Field("Dimension of Unit", UnitDimension);
        mazeWidth = EditorGUILayout.FloatField("Width in Meter", selectedMaze.MazeWidthInMeter);
        mazeLength = EditorGUILayout.FloatField("Length in Meter", selectedMaze.MazeLengthInMeter);

        EditorGUILayout.EndVertical();

        scrollPos =
                EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Width(this.position.width), GUILayout.Height(250));

        GUILayout.Label("Wall prototype", EditorStyles.boldLabel);
        wallPrototype = (GameObject)EditorGUILayout.ObjectField(wallPrototype, typeof(GameObject), true);

        if (wallPrototype != null)
        {
            if (wallPrototypeEditor == null)
                wallPrototypeEditor = Editor.CreateEditor(wallPrototype);

            wallPrototypeEditor.OnPreviewGUI(GUILayoutUtility.GetRect(150, 150), EditorStyles.whiteLabel);
        }

        
        GUILayout.Label("Edge prototype", EditorStyles.boldLabel);
        edgePrototype = (GameObject)EditorGUILayout.ObjectField(edgePrototype, typeof(GameObject), true);

        if (edgePrototype != null)
        {
            if (edgePrototypeEditor == null)
                edgePrototypeEditor = Editor.CreateEditor(edgePrototype);

            edgePrototypeEditor.OnPreviewGUI(GUILayoutUtility.GetRect(150, 150), EditorStyles.whiteLabel);
        }

        EditorGUILayout.EndScrollView();

        EditorGUILayout.BeginVertical();
        showWayPointOptions = EditorGUILayout.Foldout(showWayPointOptions, "Waypoints");
        
        if (showWayPointOptions) {
            CreateWaypoints = EditorGUILayout.BeginToggleGroup("Create Waypoints", CreateWaypoints);

            EditorGUILayout.EndToggleGroup();

        }

        if (GUILayout.Button("Configure Maze"))
        {
            UpdateTheSelectedMaze();
        }
        
        EditorGUILayout.EndVertical();
    }


    private void UpdateTheSelectedMaze(){

        unitColumns = Mathf.FloorToInt( mazeWidth / UnitDimension.x);
        unitRows = Mathf.FloorToInt( mazeLength / UnitDimension.y);

        if ((selectedMaze.Walls == null && selectedMaze.Edges == null) || selectedMaze.Edges.Count == 0 && selectedMaze.Walls.Count == 0)
            {
                // TODO Generate a plain maze
                selectedMaze.Walls = new System.Collections.Generic.List<GameObject>();
                selectedMaze.Edges = new System.Collections.Generic.List<GameObject>();
                GeneratePlainMaze();
            }
            else 
            {
                // Update the already existing maze
                // iterating over the existing units and update the parameter
            }
    }

    private beMobileMaze GeneratePlainMaze()  
    {
        beMobileMaze mazeHost = selectedMaze.gameObject.GetComponent<beMobileMaze>();

        Vector2 unitDimension = UnitDimension;

        //float starPointX = mazeHost.transform.position.x - WallThickness / 2 - selectedMaze.MazeWidthInMeter / 2;
        //float startPointY = mazeHost.transform.position.z - WallThickness / 2 - selectedMaze.MazeLengthInMeter / 2;
        //float floor = mazeHost.transform.position.y;
        //Vector3 startPosition =  new Vector3(starPointX, floor, startPointY);
 
        //for (int col = 0; col < unitColumns; col++)
        //{
        //    for (int row = 0; row < unitRows; row++)
        //    {
        //        Vector3 unitPosition = startPosition + new Vector3(col * unitDimension.x, floor, row * unitDimension.y);
        //        GameObject unit = CreateUnitWithParentAt(string.Format(UnitNamePattern, col, row), selectedMaze.transform, unitPosition);
        //        mazeHost.Units.Add(unit);
                 
        //        /** Create between row & col
        //         * 
        //         *  |
        //         *  #--
        //         * 
        //         **/

        //        Vector3 targetPosition = unit.transform.position;
        //        Vector3 targetColumnWallPosition = targetPosition + new Vector3(WallThickness, floor, WallThickness);
        //        Vector3 targetRowWallPosition = targetPosition + new Vector3(0, floor, WallThickness);


        //        GameObject edge = (GameObject)GameObject.Instantiate(edgePrototype, targetPosition, Quaternion.identity);
        //        edge.transform.parent = unit.transform;
        //        GameObject colWall = (GameObject)GameObject.Instantiate(wallPrototype, targetColumnWallPosition,  Quaternion.Euler(0,90,0));
        //        colWall.transform.parent = unit.transform;

        //        GameObject rowWall = (GameObject)GameObject.Instantiate(wallPrototype, targetRowWallPosition, Quaternion.identity);
        //        rowWall.transform.parent = unit.transform;
                
        //        /** Create last column n-1
        //         * 
        //         * --#
        //         *   |
        //         * --#
        //         *   |
        //         * --#
        //         *   
        //         **/
        //        if (col == unitColumns -1)
        //        {   
        //            Vector3 targetLastColumnEdgePosition = targetColumnWallPosition + new Vector3(WallLength, floor, -WallThickness);

        //            GameObject lastColedge = (GameObject)GameObject.Instantiate(edgePrototype, targetLastColumnEdgePosition, Quaternion.identity);
        //            lastColedge.transform.parent = unit.transform;

        //            Vector3 targetLastColumnWallPosition = targetLastColumnEdgePosition + new Vector3(0, floor, WallThickness);
        //            GameObject lastColWall = (GameObject)GameObject.Instantiate(wallPrototype, targetLastColumnWallPosition, Quaternion.identity);
        //            lastColWall.transform.parent = unit.transform;

        //            if (row == unitRows - 1) { 
        //                Vector3 targetLastColumRowEdge = targetLastColumnWallPosition + new Vector3(0, floor, WallLength);
        //                GameObject lastColRowEdge = (GameObject)GameObject.Instantiate(edgePrototype, targetLastColumRowEdge, Quaternion.identity);
        //                lastColRowEdge.transform.parent = unit.transform;
        //            }
        //        }

        //       /** Create last row n-1
        //        * 
        //        * #--#--#
        //        * |  |  |
        //        * #--#--#
        //        * |  |  |
        //        **/
        //        if (row == unitRows - 1)
        //        {
        //            Debug.Log("column " + col + " last row reached");
        //            Vector3 lastEdgePosition = targetPosition + new Vector3(0, floor, WallLength + WallThickness);
        //            GameObject lastEdge = (GameObject)GameObject.Instantiate(edgePrototype, lastEdgePosition, Quaternion.identity);
        //            lastEdge.transform.parent = unit.transform; 

        //            Vector3 lastWallPosition = lastEdgePosition + new Vector3(WallThickness, 0, WallThickness);
        //            GameObject lastRowWall = (GameObject)GameObject.Instantiate(wallPrototype, lastWallPosition, Quaternion.Euler(0, 90, 0));
        //            lastRowWall.transform.parent = unit.transform;
        //        }

        //    }
        //}
        
        return mazeHost;
    }

    private GameObject CreateUnitWithParentAt(string name, Transform parent, Vector3 position)
    {
        GameObject unit = new GameObject(name);
        unit.transform.parent = parent;
        unit.transform.position = position;
        return unit;
    } 

    
}