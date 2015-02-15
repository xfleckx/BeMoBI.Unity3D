using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class MazeEditorWindow : EditorWindow
{

    string MazeName = "beMoBI.Maze";
    string Description = "A example maze";
    bool CreateWaypoints = true;
    
    float WallThickness = 0.1f;
    Vector2 UnitDimension = new Vector2(1.5f,1.5f);
    float mazeWidth = 0;
    float mazeLength = 0;

    int unitsInLength = 0;
    int unitsInWidth = 0;

    private beMobileMaze selectedMaze;

    private GameObject wallPrototype;
    private Editor wallPrototypeEditor;

    private GameObject edgePrototype;
    private Editor edgePrototypeEditor;

    private string UnitNamePattern = "{0}.{1}";

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
        GUILayout.Label("Base Settings", EditorStyles.boldLabel);
        MazeName = EditorGUILayout.TextField("Name:", selectedMaze.name);
        Description = EditorGUILayout.TextField("Description:", Description);
        WallThickness = EditorGUILayout.Slider("Wall thickness", WallThickness, 0.01f, 0.9f);
        UnitDimension = EditorGUILayout.Vector2Field("Dimension of Unit", UnitDimension);
        mazeWidth = EditorGUILayout.FloatField("Width in Meter", selectedMaze.MazeWidthInMeter);
        mazeLength = EditorGUILayout.FloatField("Length in Meter", selectedMaze.MazeLengthInMeter);
  
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

        unitsInWidth = Mathf.FloorToInt( mazeWidth / UnitDimension.x);
        unitsInLength = Mathf.FloorToInt( mazeLength / UnitDimension.y);

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

        float starPointX = mazeHost.transform.position.x - selectedMaze.MazeWidthInMeter / 2;
        float startPointY = mazeHost.transform.position.z - selectedMaze.MazeLengthInMeter / 2;
        Vector3 startPosition =  new Vector3(starPointX, 0, startPointY);
 
        for (int i = 0; i < unitsInWidth; i++)
        {
            for (int j = 0; j < unitsInLength; j++)
            {  
                Vector3 unitPosition = startPosition + new Vector3(i * unitDimension.x, 0, j * unitDimension.y);
                GameObject unit = CreateUnitWithParentAt(string.Format(UnitNamePattern, i, j), selectedMaze.transform, unitPosition);
                mazeHost.Units.Add(unit);
            }
        }
        
        return mazeHost;
    }

    private GameObject CreateUnitWithParentAt(string name, Transform parent, Vector3 position) 
    {
         GameObject unit = new GameObject(name);
                unit.transform.parent = parent;
                unit.transform.position = position; 
                return unit;
    }

    private object CreateNewEdgeAt(float ColOffset, float RowOffset)
    {
        throw new System.NotImplementedException();
    }

    private object CreateNewWallAt(float ColOffset, float RowOffset)
    {
        throw new System.NotImplementedException();
    }

    
}