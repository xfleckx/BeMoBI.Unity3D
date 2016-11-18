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
            if (VERBOSE)
                Debug.Log(message);
        }

        // Add menu named "My Window" to the Window menu
        [MenuItem("SNEED/Maze Workflow")]
        static void Init()
        {
            // Get existing open window or if none, make a new one:
            var window = EditorWindow.GetWindow<MazeCreationWorkflow>();

            var backend = FindOrCreateBackend();

            window.Initialize(backend);
            window.titleContent = new GUIContent("Workflow");

            window.Show();
        }

        const bool VERBOSE = true;

        private MazeCreationWorkflowBackEnd backend;
        
        static MazeCreationWorkflowBackEnd FindOrCreateBackend()
        {
            var existingInstance = FindObjectOfType<MazeCreationWorkflowBackEnd>();

            if (existingInstance != null) {
                log("Using existing backend instance");

                return existingInstance;
            }

            log("Create bew Backend instance");

            var newInstance = CreateInstance<MazeCreationWorkflowBackEnd>();

            newInstance.OnEditorModeChange += m => log("Change to Mode: " +m.Name);

            return newInstance;
        }

        private void Initialize(MazeCreationWorkflowBackEnd currentBackend)
        {
            if (backend != currentBackend)
            {
                DestroyImmediate(backend);
                backend = currentBackend;
            }

            backend.CheckCurrentSelection();
        }

        private void OnGUI()
        {
            GUILayout.Label("Selected Maze: " + backend.getMazeName());

            if (backend.selectedMazeHasAPrefab())
            {
                EditorGUILayout.HelpBox("It has a prefab!", MessageType.Info);
            }

            if (GUILayout.Button("Create new Maze"))
            {

            }

            if (backend.hasNoMazeSelected())
            {
                return;
            }

            GUILayout.Label("Edit your Maze: ", EditorStyles.largeLabel);

            foreach (var mode in backend.EditorModes)
            {
                mode.Selected = GUILayout.Toggle(mode.Selected, mode.Name, "Button");

                if (mode.Selected && mode != backend.CurrentSelectedMode)
                    backend.ChangeEditorModeTo(mode);

                if (mode.Selected)
                    mode.OnGUI();
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