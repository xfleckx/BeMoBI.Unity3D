using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using Assets.SNEED.Unity3D.Editor.Maze;

public class MazeEditorWindow : EditorWindow
{
    private EditorState state;

    private MazeBaker mazeBaker;


    public void Initialize(EditorState editorState)
    {
        titleContent = new GUIContent("Maze Editor");
        
        this.mazeBaker = new MazeBaker();
        
        this.state = editorState;
    }


    void OnGUI()
    {
        if(state.SelectedMaze == null)
        {
            EditorGUILayout.HelpBox("No maze Selected", MessageType.Error);
            return;
        }

        EditorGUILayout.BeginVertical();

        EditorGUILayout.LabelField("(1) Add a unit prefab!", EditorStyles.boldLabel);

        state.UnitPrefab = EditorGUILayout.ObjectField("Unit Prefab:", state.UnitPrefab, typeof(GameObject), false) as GameObject;

        if (state.UnitPrefab != null)
        {
           EditorGUILayout.LabelField(AssetDatabase.GetAssetPath(state.UnitPrefab));
        } 
        


        EditorGUILayout.LabelField("(2) Define Maze Dimensions!", EditorStyles.boldLabel);

        state.MazeWidth = EditorGUILayout.FloatField("Widht", state.MazeWidth);

        state.MazeLength = EditorGUILayout.FloatField("Length", state.MazeLength);

        if(GUILayout.Button("Set Dimensions"))
        {
            state.SelectedMaze.MazeWidthInMeter = state.MazeWidth;
            state.SelectedMaze.MazeLengthInMeter = state.MazeLength;

            MazeEditorUtil.RebuildGrid(state.SelectedMaze);
        }

        EditorGUILayout.EndVertical();
        
        EditorGUILayout.LabelField("(3) Add units to the maze!", EditorStyles.boldLabel);

        #region Editing Mode UI

        if (state.SelectedMaze != null && state.UnitPrefab != null && state.SelectedMaze.DoesNotContainPaths())
        {
            state.EditingModeEnabled = GUILayout.Toggle(state.EditingModeEnabled, "Editing Mode");

            if (state.EditingModeEnabled)
            {
                if (state.ActiveMode != MazeEditorMode.EDITING)
                {
                    state.DisableModesExcept(MazeEditorMode.EDITING);
                    state.EditorModeProcessEvent = null;
                    state.EditorModeProcessEvent += state.EditingMode;
                    state.ActiveMode = MazeEditorMode.EDITING;
                }
                GUILayout.BeginHorizontal();
                GUILayout.Space(15f);
                GUILayout.BeginVertical();
                state.modeAddEnabled = GUILayout.Toggle(!state.modeRemoveEnabled, "Adding Cells");
                state.modeRemoveEnabled = GUILayout.Toggle(!state.modeAddEnabled, "Erasing Cells");
                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
            }
            else
            {
                state.modeRemoveEnabled = false;
                state.modeAddEnabled = false;
                state.EditorModeProcessEvent -= state.EditingMode;
            }
        }
        else
        {
            GUILayout.Label("Editing disabled, \n Maze contains paths!");
        }
        #endregion

        GUILayout.Space(10f);

        EditorGUILayout.LabelField("(4) Use selection to connect units!", EditorStyles.boldLabel);

        state.SelectionModeEnabled = GUILayout.Toggle(state.SelectionModeEnabled, "Selection Mode");

        #region Selection Mode UI

        if (state.SelectionModeEnabled)
        {

            if (state.ActiveMode != MazeEditorMode.SELECTION)
            {
                state.DisableModesExcept(MazeEditorMode.SELECTION);

                state.CurrentSelection = new HashSet<GameObject>();

                state.EditorModeProcessEvent = null;
                state.EditorModeProcessEvent += state.SelectionMode;

                state.ActiveMode = MazeEditorMode.SELECTION;
            }

            if (GUILayout.Button("Connect", GUILayout.Width(100f)))
            {
                state.TryConnectingCurrentSelection();
            }

            if (GUILayout.Button("Disconnect", GUILayout.Width(100f)))
            {
                state.TryDisconnectingCurrentSelection();
            }
        }
        else
        {
            state.EditorModeProcessEvent -= state.SelectionMode;

            if (state.CurrentSelection != null)
                state.CurrentSelection.Clear();
        }
        #endregion


        GUILayout.Space(10f);

        EditorGUILayout.LabelField("(4) Bake the Bake to a single Mesh!", EditorStyles.boldLabel);


        if (GUILayout.Button("Bake"))
        {
            mazeBaker.Bake(state.SelectedMaze);
        }

    }

}
