using Assets.SNEED.EditorExtensions.Maze.UnitCreation;
using Assets.SNEED.Mazes;
using Assets.SNEED.Unity3D.EditorExtensions.Maze;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Assets.SNEED.EditorExtensions.Maze
{
    public class MazeEditorWindow : EditorWindow
    {
        private const string URL_WIKI = "https://github.com/xfleckx/SNEED/wiki/Create-a-Maze";

        private const string EDPREF_LASTUSEDPATHEXPORTMAZEDATA = "lastUsedPathToExportMazeData";

        private EditorState state;
        
        private MazeBaker mazeBaker;
        private bool showExperimentalFeatures = false;

        private GUIContent savePrefabHeader = new GUIContent("(4) Save the Maze", "Mazes should be safed as prefabs before they get used in scenes.");

        public void Initialize(EditorState editorState)
        {
            titleContent = new GUIContent("Maze Editor");

            this.mazeBaker = new MazeBaker();

            this.state = editorState;

            this.state.pathToExportModelData = EditorPrefs.GetString(EDPREF_LASTUSEDPATHEXPORTMAZEDATA);

            editorState.EditorWindowVisible = true;
        }
        
        void OnGUI()
        {
            if (state == null) // the case that the Unity Editor remember the window
            {
                state = EditorState.Instance;
            }

            state.EditorWindowVisible = true;

            Event currentEvent = Event.current;

            if (currentEvent.type == EventType.ContextClick)
            {
                //Vector2 mousePos = currentEvent.mousePosition ; not used
                GenericMenu menu = new GenericMenu();
                menu.AddItem(new GUIContent("Need Help"), false, 
                    o => {
                    Application.OpenURL(URL_WIKI); },
                    "item1");
                menu.ShowAsContext();
                currentEvent.Use(); 
            }

            if (state.SelectedMaze == null)
            {
                EditorGUILayout.HelpBox("No maze Selected", MessageType.Error);
                return;
            }
            
            if (mazeBaker == null)
                mazeBaker = new MazeBaker();

            if (state.prefabOfSelectedMaze == null)
                state.CheckPrefabConnections();

            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField("(1) Define Maze Dimensions!", EditorStyles.boldLabel);

            state.MazeWidth = EditorGUILayout.FloatField("Widht", state.MazeWidth);

            state.MazeLength = EditorGUILayout.FloatField("Length", state.MazeLength);

            if (state.UnitDimensions == Vector3.zero)
                state.UnitDimensions = state.SelectedMaze.RoomDimension;

            state.UnitDimensions = EditorGUILayout.Vector3Field("Room WxHxL (m):", state.UnitDimensions, null);

            if (!state.SelectedMaze.Units.Any())
            {
                if (GUILayout.Button("Set Dimensions"))
                {
                    state.SelectedMaze.MazeWidthInMeter = state.MazeWidth;
                    state.SelectedMaze.MazeLengthInMeter = state.MazeLength;
                    state.SelectedMaze.RoomDimension = state.UnitDimensions;

                    MazeEditorUtil.RebuildGrid(state.SelectedMaze);
                }
            }else
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.HelpBox("Can't change dimensions after Units are added!", MessageType.Info);

                if(GUILayout.Button("Shrink to \n minimal Rows/Cols"))
                {
                  MazeEditorUtil.Shrink(state.SelectedMaze);
                  
                  MazeEditorUtil.RebuildGrid(state.SelectedMaze);

                  state.MazeWidth = state.SelectedMaze.MazeWidthInMeter;
                  state.MazeLength = state.SelectedMaze.MazeLengthInMeter;
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.LabelField("(2) Add a unit prefab!", EditorStyles.boldLabel);

            state.UnitPrefab = EditorGUILayout.ObjectField("Unit Prefab:", state.UnitPrefab, typeof(GameObject), false) as GameObject;

            if (state.UnitPrefab != null)
            {
                EditorGUILayout.LabelField(AssetDatabase.GetAssetPath(state.UnitPrefab));
            }
            else
            {
                EditorGUILayout.BeginHorizontal();

                EditorGUILayout.HelpBox("First add an Unit prefab!", MessageType.Info);
                
                if (GUILayout.Button("Open Unit creator", GUILayout.Height(35)))
                {
                    UnitCreator.OpenUnitCreator(state.UnitDimensions, c => {
                        // get the created Unit prefab automaticaly back to the Editor Window
                        c.onUnitPrefabCreated += prefab =>
                        {
                            var mazeUnit = prefab.GetComponent<MazeUnit>();

                            if (mazeUnit != null)
                            {
                                state.UnitPrefab = prefab;
                                this.Repaint();
                            }
                        };
                        
                    });
                }

                EditorGUILayout.EndHorizontal();
            }



            EditorGUILayout.EndVertical();

            EditorGUILayout.LabelField("(3) Add units to the maze!", EditorStyles.boldLabel);

            #region Editing Mode UI

            if (state.UnitPrefab == null)
            {
                EditorGUILayout.HelpBox("You have to add a Unit prefab!", MessageType.Warning);
            }
            else if (!state.SelectedMaze.DoesNotContainPaths())
            {
                EditorGUILayout.HelpBox("Editing disabled, \n Maze contains paths!", MessageType.Warning);
            }
            else
            {
                RenderEditorModeOptions();
            }
            #endregion

            GUILayout.Space(10f);

            EditorGUILayout.LabelField("(4) Use selection to connect units!", EditorStyles.boldLabel);

            state.SelectionModeEnabled = GUILayout.Toggle(state.SelectionModeEnabled, "Selection Mode", "Button");

            #region Selection Mode UI

            if (state.SelectionModeEnabled)
            {

                if (state.ActiveMode != MazeEditorMode.SELECTION)
                {
                    state.DisableModesExcept(MazeEditorMode.SELECTION);

                    state.CurrentSelection = new List<GameObject>();

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

            GUILayout.Label(savePrefabHeader, EditorStyles.boldLabel);

            if (state.prefabOfSelectedMaze != null && GUILayout.Button("Save prefab"))
            {
                PrefabUtility.ReplacePrefab(state.SelectedMaze.gameObject, state.prefabOfSelectedMaze, ReplacePrefabOptions.ConnectToPrefab);

                EditorUtility.SetDirty(state.prefabOfSelectedMaze);
            }

            if (GUILayout.Button("Save as new prefab"))
            {
                var filePath = EditorUtility.SaveFilePanel("Save as prefab", Application.dataPath, "prefabName", "prefab");

                if (filePath != null)
                {
                    if (File.Exists(filePath))
                    {
                        var relativeFileName = filePath.Replace(Application.dataPath, "Assets");

                        state.referenceToPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(relativeFileName);

                        state.prefabOfSelectedMaze = PrefabUtility.ReplacePrefab(state.SelectedMaze.gameObject, state.prefabOfSelectedMaze);
                    }
                    else
                    {
                        var relativeFileName = filePath.Replace(Application.dataPath, "Assets");

                        state.prefabOfSelectedMaze = PrefabUtility.CreatePrefab(relativeFileName, state.SelectedMaze.gameObject);
                    }

                    EditorUtility.SetDirty(state.prefabOfSelectedMaze);
                }

            }


            GUILayout.Space(10f);

            showExperimentalFeatures = EditorGUILayout.Foldout(showExperimentalFeatures, "Experimental Features");

            if (showExperimentalFeatures)
            {
                EditorGUILayout.LabelField("Bake the Bake to a single Mesh!", EditorStyles.boldLabel);

                mazeBaker.replaceOriginalMaze = EditorGUILayout.Toggle("Replace the original maze", mazeBaker.replaceOriginalMaze);

                mazeBaker.ignoreFloor = EditorGUILayout.Toggle("Remove floor on baking", mazeBaker.ignoreFloor);

                if (GUILayout.Button("Bake"))
                {
                    state.finalizedMaze = mazeBaker.Bake(state.SelectedMaze);
                }

                if (state.finalizedMaze != null && GUILayout.Button("Safe baked maze as new prefab"))
                {
                    var recommendedPath = EditorEnvironmentConstants.Get_PACKAGE_PREFAB_SUBFOLDER() + "/" + state.finalizedMaze.name + ".prefab";

                    var chooseAnotherLocation = EditorUtility.DisplayDialog("Use default location?", "Using default location for these prefabs? \n " + recommendedPath, "No let me choose one", "Yes");

                    if (chooseAnotherLocation)
                    {
                        recommendedPath = EditorUtility.SaveFilePanel("Save prefab", Application.dataPath, state.finalizedMaze.name, "prefab");
                    }

                    if (recommendedPath == null)
                    {
                        Debug.Log("You have to choose a path to save the prefab!");
                        return;
                    }


                    PrefabUtility.CreatePrefab(recommendedPath, state.finalizedMaze.gameObject);
                }
            }
           

            GUILayout.Space(10f);


            EditorGUILayout.BeginVertical();

            EditorGUILayout.LabelField("(5) Export Maze Model", EditorStyles.boldLabel);
            
            EditorGUILayout.EndVertical();
            
            state.pathToExportModelData = EditorGUILayout.TextField("Directory:", state.pathToExportModelData);

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Choose Directory to Export"))
            {
                state.pathToExportModelData = EditorUtility.OpenFolderPanel("Choose directory to export a maze model", Environment.CurrentDirectory, "");
            }

            if (Directory.Exists(state.pathToExportModelData) && state.SelectedMaze != null && GUILayout.Button("Export"))
            {
                var exporter = new SimpleTextFileMazeExporter();

                exporter.UnexpectedValuesFound += message =>
                {
                    return EditorUtility.DisplayDialog("Problem during Export", message, "Abort...");
                };

                var targetFileName = exporter.CreateTargetFileName(state.SelectedMaze);

                var targetFile = new FileInfo(Path.Combine(state.pathToExportModelData,targetFileName));

                exporter.Export(state.SelectedMaze, targetFile);

                EditorPrefs.SetString(EDPREF_LASTUSEDPATHEXPORTMAZEDATA, state.pathToExportModelData);
            }

            EditorGUILayout.EndHorizontal();
        }

        public void OnSelectionChange()
        {
            if (Selection.activeGameObject == null)
                return;

            var newSelectedMaze = Selection.activeGameObject.GetComponent<beMobileMaze>();

            if (newSelectedMaze == null)
                return;

            if(state.SelectedMaze != null && state.SelectedMaze != newSelectedMaze)
                state.Initialize(newSelectedMaze);

        }

        private void RenderEditorModeOptions()
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

        void OnDestroy()
        {
            if (state != null)
                state.EditorWindowVisible = false;

            
        }
    }
}