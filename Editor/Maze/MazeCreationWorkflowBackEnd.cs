using UnityEngine;
using UnityEditor;
using System;
using Assets.SNEED.Mazes;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Assets.SNEED.EditorExtensions;
using Assets.SNEED.EditorExtensions.Maze;

namespace Assets.SNEED.EditorExtension.Maze
{
    public class MazeCreationWorkflowBackEnd : ScriptableObject
    {
        public static MazeCreationWorkflowBackEnd Instance;

        #region Unity Messages
        private void OnEnable()
        {
            Instance = this;

            LoadEditorModeOptions();

            visual = new EditorViewGridVisualisation();

            SceneView.onSceneGUIDelegate -= onSceneGUI;
            SceneView.onSceneGUIDelegate += onSceneGUI;

            EditorApplication.update -= onEditorApplicationUpdate;
            EditorApplication.update += onEditorApplicationUpdate;

        }

        private void onSceneGUI(SceneView view)
        {
            if (hasNoMazeSelected())
                return;

            visual.CalculateTilePosition(selectedMaze.transform, selectedMaze.RoomDimension, selectedMaze.Rows, selectedMaze.Columns);
            
            if (EditorApplication.isPlaying)
                return;

            visual.renderDebugInfos();
            
            if (CurrentSelectedMode != null)
                CurrentSelectedMode.OnSceneViewGUI(view, this, visual);

            var cam = Camera.current;

            if (GridEditingVisualUtils.CameraIsATopDownView(cam, selectedMaze.transform) && cam.orthographic)
            { 
                // special case for Schematic view
            }

            var currentEvent = Event.current;

            if(currentEvent.type == EventType.MouseUp) {
                if(currentEvent.button == 0)
                { 
                    Debug.Log("klick");
                }
            }
        }

        private void onEditorApplicationUpdate()
        {
            
        }

        private void OnDisable()
        {

        }

        private void OnDestroy()
        {
            EditorApplication.update -= onEditorApplicationUpdate;
            SceneView.onSceneGUIDelegate -= onSceneGUI;
            Debug.Log("Backend destroyed");
        }

        #endregion

        #region backend state

        internal EditorViewGridVisualisation visual;

        private static SchematicMazeRenderer schemaRenderer;

        internal beMobileMaze selectedMaze;

        internal List<IEditorMode> EditorModes;

        internal IEditorMode CurrentSelectedMode;

        internal Action<IEditorMode> OnEditorModeChange;
        
        private GameObject referenceToPrefab;
        private UnityEngine.Object prefabOfSelectedMaze;
        private Vector3 lastMouseHit;
        private Vector3 MarkerPosition;

        private void LoadEditorModeOptions()
        {
            EditorModes = new List<IEditorMode>();

            EditorModes.Add(new DrawingMode());
            EditorModes.Add(new MergeAndSeparateMode());
            EditorModes.Add(new PathCreationMode());
        }
        
        internal void CheckCurrentSelection()
        {
            var currentSelection = Selection.activeGameObject.GetComponent<beMobileMaze>();

            if (currentSelection != null)
            {
                if(selectedMaze != currentSelection)
                {
                    InitializeWithMaze(currentSelection);
                }
            }
        }
        #endregion

        private void InitializeWithMaze(beMobileMaze currentSelection)
        {
            selectedMaze = currentSelection;

            prefabOfSelectedMaze = PrefabUtility.GetPrefabParent(currentSelection);
            string path = AssetDatabase.GetAssetPath(prefabOfSelectedMaze);
        }

        internal bool selectedMazeHasAPrefab()
        {
            return prefabOfSelectedMaze;
        }

        internal string getMazeName()
        {
            if (selectedMaze != null)
                return selectedMaze.name;

            return "none";
        }

        internal bool hasNoMazeSelected()
        {
            return selectedMaze == null;
        }

        internal void CheckModeSelection()
        {
            if (EditorModes.All(m => !m.Selected) )
            {
                if(CurrentSelectedMode != null) { 
                    ResetEditorMode(CurrentSelectedMode);
                    CurrentSelectedMode = null;
                }

                visual.CurrentHighlightingColor = Color.clear;

                return;
            }
            
            var selected = EditorModes.Where(m => m.Selected);

            if (!selected.Any( m => m.Equals(CurrentSelectedMode) ))
            {
                // reset mode to none
                ResetEditorMode(selected.First());
            }

            if(selected.Any(m => m.Equals(CurrentSelectedMode)))
            {
                var allNotSelectedMmodes = selected.Where(m => m.Equals(CurrentSelectedMode));

                foreach (var item in allNotSelectedMmodes)
                {
                    ResetEditorMode(item);
                }
            }

        }

        internal void SaveToPrefab(string filePath)
        {
            if (File.Exists(filePath))
            {
                var relativeFileName = filePath.Replace(Application.dataPath, "Assets");

                referenceToPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(relativeFileName);

                prefabOfSelectedMaze = PrefabUtility.ReplacePrefab(selectedMaze.gameObject, prefabOfSelectedMaze);
            }
            else
            {
                var relativeFileName = filePath.Replace(Application.dataPath, "Assets");

                prefabOfSelectedMaze = PrefabUtility.CreatePrefab(relativeFileName, selectedMaze.gameObject);
            }

            EditorUtility.SetDirty(prefabOfSelectedMaze);
        }

        internal void ChangeEditorModeTo(IEditorMode mode)
        {
            ResetEditorMode(CurrentSelectedMode);

            var newSelectedMode = EditorModes.Where(m => m.Selected);

            changeTo(newSelectedMode.First());
        }

        /// <summary>
        /// do mode initialization here
        /// </summary>
        /// <param name="newSelectedMode"></param>
        private void changeTo(IEditorMode newSelectedMode)
        {
            CurrentSelectedMode = newSelectedMode;

            visual.CurrentHighlightingColor = CurrentSelectedMode.GetPrimaryColor();

            if(OnEditorModeChange != null)
                OnEditorModeChange(CurrentSelectedMode);
        }

        /// <summary>
        /// Remove all callbackes from mode
        /// </summary>
        /// <param name="mode"></param>
        private void ResetEditorMode(IEditorMode mode = null)
        {
            if(mode == null)
            {
                // Clear all callbacks and reset saved states of the modes
                
                return;
            }

            mode.Selected = false;
        }
        
        [DrawGizmo(GizmoType.Active | GizmoType.InSelectionHierarchy, typeof(beMobileMaze))]
        static void DrawGizmosFor(beMobileMaze maze, GizmoType type)
        {

            Instance.visual.RenderTileHighlighting(maze.transform, maze.RoomDimension * 1.1f);


            //RenderEditorGizmos(maze, editorState);

        }
    }
}