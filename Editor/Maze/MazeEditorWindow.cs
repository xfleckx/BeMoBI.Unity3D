using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using Assets.Daedalus.Unity3D.Editor.Maze;

public class MazeEditorWindow : EditorWindow
{
    [SerializeField]
    private beMobileMaze maze;
    
    private MazeInspector inspector;

    private MazeBaker mazeBaker;

    private int MazeWidth;

    private int MazeLength;

    public void Initialize(beMobileMaze maze, MazeInspector inspector)
    {
        titleContent = new GUIContent("Maze Editor");

        this.maze = maze;

        this.mazeBaker = new MazeBaker();

        this.inspector = inspector;
    }


    void OnGUI()
    {
        if(inspector == null || maze == null)
        {
            EditorGUILayout.HelpBox("No maze Selected", MessageType.Error);
            return;
        }

        EditorGUILayout.BeginVertical();

        EditorGUILayout.LabelField("(1) Add a unit prefab!", EditorStyles.boldLabel);

        inspector.UnitPrefab = EditorGUILayout.ObjectField("Unit Prefab:", inspector.UnitPrefab, typeof(GameObject), false) as GameObject;

        if (inspector.UnitPrefab != null)
        {
           EditorGUILayout.LabelField(AssetDatabase.GetAssetPath(inspector.UnitPrefab));
        } 
        
        EditorGUILayout.LabelField("(2) Define Maze Dimensions!", EditorStyles.boldLabel);

        MazeWidth = EditorGUILayout.IntField("Widht", MazeWidth);

        MazeLength = EditorGUILayout.IntField("Length", MazeLength);

        if(GUILayout.Button("Set Dimensions"))
        {
            maze.MazeWidthInMeter = MazeWidth;
            maze.MazeLengthInMeter = MazeLength;

            MazeEditorUtil.RebuildGrid(maze);
        }

        EditorGUILayout.EndVertical();
        
        EditorGUILayout.LabelField("(3) Add units to the maze!", EditorStyles.boldLabel);

        #region Editing Mode UI

        if (maze != null && inspector.UnitPrefab != null && inspector.MazeDoesNotContainPaths())
        {
            inspector.EditingModeEnabled = GUILayout.Toggle(inspector.EditingModeEnabled, "Editing Mode");

            if (inspector.EditingModeEnabled)
            {
                if (inspector.ActiveMode != MazeEditorMode.EDITING)
                {
                    inspector.DisableModesExcept(MazeEditorMode.EDITING);
                    inspector.EditorModeProcessEvent = null;
                    inspector.EditorModeProcessEvent += inspector.EditingMode;
                    inspector.ActiveMode = MazeEditorMode.EDITING;
                }
                GUILayout.BeginHorizontal();
                GUILayout.Space(15f);
                GUILayout.BeginVertical();
                inspector.modeAddEnabled = GUILayout.Toggle(!inspector.modeRemoveEnabled, "Adding Cells");
                inspector.modeRemoveEnabled = GUILayout.Toggle(!inspector.modeAddEnabled, "Erasing Cells");
                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
            }
            else
            {
                inspector.modeRemoveEnabled = false;
                inspector.modeAddEnabled = false;
                inspector.EditorModeProcessEvent -= inspector.EditingMode;
            }
        }
        else
        {
            GUILayout.Label("Editing disabled, \n Maze contains paths!");
        }
        #endregion

        GUILayout.Space(10f);

        EditorGUILayout.LabelField("(4) Use selection to connect units!", EditorStyles.boldLabel);

        inspector.SelectionModeEnabled = GUILayout.Toggle(inspector.SelectionModeEnabled, "Selection Mode");

        #region Selection Mode UI

        if (inspector.SelectionModeEnabled)
        {

            if (inspector.ActiveMode != MazeEditorMode.SELECTION)
            {
                inspector.DisableModesExcept(MazeEditorMode.SELECTION);

                inspector.CurrentSelection = new HashSet<GameObject>();

                inspector.EditorModeProcessEvent = null;
                inspector.EditorModeProcessEvent += inspector.SelectionMode;

                inspector.ActiveMode = MazeEditorMode.SELECTION;
            }

            if (GUILayout.Button("Connect", GUILayout.Width(100f)))
            {
                inspector.TryConnectingCurrentSelection();
            }

            if (GUILayout.Button("Disconnect", GUILayout.Width(100f)))
            {
                inspector.TryDisconnectingCurrentSelection();
            }
        }
        else
        {
            inspector.EditorModeProcessEvent -= inspector.SelectionMode;

            if (inspector.CurrentSelection != null)
                inspector.CurrentSelection.Clear();
        }
        #endregion


        GUILayout.Space(10f);

        EditorGUILayout.LabelField("(4) Bake the Bake to a single Mesh!", EditorStyles.boldLabel);


        if (GUILayout.Button("Bake"))
        {
            mazeBaker.Bake(maze);
        }

    }

    void OnDestroy()
    {
    }
}
