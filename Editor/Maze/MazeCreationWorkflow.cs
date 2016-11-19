using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Assets.SNEED.EditorExtension.Maze
{
    public class MazeCreationWorkflow : EditorWindow
    {
        const bool VERBOSE = true;

        private static void log(string message)
        {
            if (VERBOSE)
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


            GUILayout.Label("Selected Maze: " + backend.getMazeName());

            if (backend.selectedMazeHasAPrefab() && GUILayout.Button("Select it's prefab"))
            {
                Selection.activeObject = backend.GetMazePrefab();
            }
            
            if(GUILayout.Button("Create new Maze"))
            {
                backend.mazeCreationMode.Selected = true;
                backend.mazeCreationMode.CreateNewPlainMaze();
            }

            GUILayout.Label("Edit your Maze: ", EditorStyles.largeLabel);

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

            if(GUILayout.Button("Save Prefab")){

                var filePath = EditorUtility.SaveFilePanel("Save as prefab", Application.dataPath, "prefabName", "prefab");

                if (filePath != null)
                {
                    backend.SaveToPrefab(filePath);
                }
            }
            
            if(GUILayout.Button("Export Maze"))
            {

            }

            EditorGUILayout.EndScrollView();
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