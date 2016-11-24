using Assets.SNEED.EditorExtensions.Maze;
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Assets.SNEED.EditorExtension.Maze
{
    public class MazeCreationWorkflow : EditorWindow
    {
        private static void log(string message)
        {
            if (SNEEDPreferences.debugVerbosity)
                Debug.Log(message);
        }


        [MenuItem("SNEED/Maze Workflow")]
        static void Init()
        {
            // Get existing open window or if none, make a new one:
            var window = EditorWindow.GetWindow<MazeCreationWorkflow>();

            window.titleContent = new GUIContent("Workflow");

            window.Show();
        }

        private MazeCreationWorkflowBackEnd backend;
        
        static MazeCreationWorkflowBackEnd FindOrCreateBackend()
        {
            var existingInstance = ScriptableObject.FindObjectOfType<MazeCreationWorkflowBackEnd>();

            if (existingInstance != null) {
                log("Using existing backend instance");

                return existingInstance;
            }

            log("Create bew Backend instance");

            var newInstance = CreateInstance<MazeCreationWorkflowBackEnd>();

            newInstance.OnEditorModeChange += m => log("Change to Mode: " +m.Name);

            return newInstance;
        }

        private void OnEnable()
        {
            log("Workflow Enabled");

            var availableBackend = FindOrCreateBackend();
            
            if (backend != null && backend.GetInstanceID() != availableBackend.GetInstanceID())
            {
                log("Found old backend - destroy it...");
                DestroyImmediate(backend);
                backend = null;
            }

            backend = availableBackend;

            backend.CheckCurrentSelection();
        }

        private Vector2 scrollPosition;
        private bool showExportOptions;

        private void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);


            if (backend.hasNoMazeSelected())
            {
                backend.mazeCreationMode.Selected = GUILayout.Toggle(backend.mazeCreationMode.Selected, backend.mazeCreationMode.Name, "Button");
                
                if(backend.mazeCreationMode.Selected)
                {
                    backend.mazeCreationMode.OnGUI(backend);
                }
            }

            if(backend.hasNoMazeSelected())
            {
                EditorGUILayout.HelpBox("Create or Select a Maze!", MessageType.Info, true);
                EditorGUILayout.EndScrollView();
                return;
            }

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Create\nnew Maze", GUILayout.Width(80), GUILayout.Height(50)))
            {
                backend.mazeCreationMode.Selected = true;
                backend.mazeCreationMode.CreateNewPlainMaze(backend);
            }
            EditorGUILayout.BeginVertical();

            GUILayout.Label("Selected Maze: " + backend.getMazeName(), EditorStyles.largeLabel);

            if (backend.selectedMazeHasAPrefab() && GUILayout.Button("Select it's prefab"))
            {
                Selection.activeObject = backend.GetMazePrefab();
            }

            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();

            separate();

            GUILayout.Label("Edit your Maze: ", EditorStyles.largeLabel);

            if (!backend.mazeHasUnits())
            {
                backend.mazeCreationMode.Selected = GUILayout.Toggle(backend.mazeCreationMode.Selected, backend.mazeCreationMode.Name, "Button");

                if (backend.mazeCreationMode.Selected)
                    backend.mazeCreationMode.OnGUI(backend);
            }

            EditorGUILayout.Space();

            foreach (var mode in backend.EditorModes)
            {
                mode.Selected = GUILayout.Toggle(mode.Selected, mode.Name, "Button");

                if (mode.Selected && mode != backend.CurrentSelectedMode)
                    backend.ChangeEditorModeTo(mode);

                if (!mode.Selected && mode == backend.CurrentSelectedMode)
                    backend.ResetCurrentEditorMode();

                if (mode.Selected)
                    mode.OnGUI(backend);

                EditorGUILayout.Space();
            }
            
            EditorGUILayout.Space();


            EditorGUILayout.BeginHorizontal();

            if(backend.selectedMazeHasAPrefab() && GUILayout.Button("Save Changes"))
            {
                backend.OverridePrefab();
            }

            if(GUILayout.Button("Save Prefab")){

                var filePath = EditorUtility.SaveFilePanel("Save as prefab", Application.dataPath, "prefabName", "prefab");

                if (filePath != null && filePath != string.Empty)
                {
                    backend.SaveToPrefab(filePath);
                }
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();

            separate();

            showExportOptions = GUILayout.Toggle( showExportOptions, "Export Maze", "Button");

            if (showExportOptions)
            {
                backend.pathToExportModelData = EditorGUILayout.TextField("Directory:", backend.pathToExportModelData);

                EditorGUILayout.BeginHorizontal();

                if (GUILayout.Button("Choose Directory to Export"))
                {
                    backend.pathToExportModelData = EditorUtility.OpenFolderPanel("Choose directory to export a maze model", Environment.CurrentDirectory, "");
                }

                if(backend.exportReady() && GUILayout.Button("Export"))
                {
                    backend.ExportMazeData(message =>
                    {
                        return EditorUtility.DisplayDialog("Problem during Export", message, "Abort...");
                    });
                }

                EditorGUILayout.EndHorizontal();
            }

            
            EditorGUILayout.EndScrollView();
        }

        private void separate()
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        }

        private void OnSelectionChange()
        {
            if (Selection.activeGameObject == null)
                return;

            backend.CheckCurrentSelection();

            Repaint();
        }

        private void OnDestroy()
        {
            DestroyImmediate(backend);
        }

    }
}