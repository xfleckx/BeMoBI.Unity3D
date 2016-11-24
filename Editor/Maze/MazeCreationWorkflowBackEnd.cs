using UnityEngine;
using UnityEditor;
using System;
using Assets.SNEED.Mazes;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Assets.SNEED.EditorExtensions;
using Assets.SNEED.EditorExtensions.Maze;
using Assets.SNEED.EditorExtension.Maze.EditorModes;

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
            if (EditorApplication.isPlaying)
                return;

            if (hasNoMazeSelected())
                return;

            visual.pushHandleMatrix(selectedMaze.transform.localToWorldMatrix);
            
            mazeCreationMode.OnSceneViewGUI(view, this, visual);

            visual.popHandleMatrix();

            visual.CalculateTilePosition(selectedMaze.transform, selectedMaze.RoomDimension, selectedMaze.Rows, selectedMaze.Columns);

            visual.renderDebugInfos();

            if (CurrentSelectedMode != null)
                CurrentSelectedMode.OnSceneViewGUI(view, this, visual);
            
            if (CurrentSelectedMode != null)
                CurrentSelectedMode.ProcessEvent(Event.current, this, visual);
            
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

        internal MazeCreationMode mazeCreationMode;

        internal List<IEditorMode> EditorModes;

        internal IEditorMode CurrentSelectedMode;

        internal Action<IEditorMode> OnEditorModeChange;

        private GameObject referenceToPrefab;
        private UnityEngine.Object prefabOfSelectedMaze;
        private string pathToPrefab = "";

        private Vector3 lastMouseHit;
        private Vector3 MarkerPosition;
        internal string pathToExportModelData;

        private bool hasChangesToApply;
        public bool HasChangesToApply { get { return hasChangesToApply; } }

        private void LoadEditorModeOptions()
        {
            mazeCreationMode = new MazeCreationMode();

            EditorModes = new List<IEditorMode>();

            EditorModes.Add(new DrawingMode());
            EditorModes.Add(new MergeAndSeparateMode());
            EditorModes.Add(new PathCreationMode());
        }

        internal void CheckCurrentSelection()
        {
            var selectedObject = Selection.activeGameObject;

            if (selectedObject == null) {
                return;
            }

            var currentSelection = selectedObject.GetComponent<beMobileMaze>();

            if (currentSelection != null)
            {
                if(selectedMaze != currentSelection)
                {
                    InitializeWithMaze(currentSelection);
                }
            }
        }

        internal bool mazeHasUnits()
        {
            if (selectedMaze == null)
                return false;

            return selectedMaze.Units.Any();
        }

        internal void indicateChange()
        {
            hasChangesToApply = true;
        }

        internal void ChangesApplied()
        {
            hasChangesToApply = false;
        }
        #endregion

        private void InitializeWithMaze(beMobileMaze currentSelection)
        {
            selectedMaze = currentSelection;

            prefabOfSelectedMaze = PrefabUtility.GetPrefabParent(currentSelection);
            pathToPrefab = AssetDatabase.GetAssetPath(prefabOfSelectedMaze);
        }

        internal UnityEngine.Object GetMazePrefab()
        {
            return prefabOfSelectedMaze;
        }

        internal string getPathToPrefab()
        {
            return pathToPrefab;
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

        internal void OverridePrefab()
        {
            prefabOfSelectedMaze = PrefabUtility.ReplacePrefab(selectedMaze.gameObject, prefabOfSelectedMaze);
            ChangesApplied();
            EditorUtility.SetDirty(prefabOfSelectedMaze);
        }

        internal bool hasNoMazeSelected()
        {
            return selectedMaze == null;
        }

        internal void SaveToPrefab(string filePath)
        {
            if (filePath == string.Empty)
                return;

            if (File.Exists(filePath))
            {
                var relativeFileName = filePath.Replace(Application.dataPath, "Assets");

                referenceToPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(relativeFileName);

                prefabOfSelectedMaze = PrefabUtility.ReplacePrefab(selectedMaze.gameObject, referenceToPrefab);
            }
            else
            {
                var relativeFileName = filePath.Replace(Application.dataPath, "Assets");

                prefabOfSelectedMaze = PrefabUtility.CreatePrefab(relativeFileName, selectedMaze.gameObject);
            }

            EditorUtility.SetDirty(prefabOfSelectedMaze);
            EditorApplication.delayCall += AssetDatabase.SaveAssets;
            ChangesApplied();
        }

        internal void ExportMazeData(Func<string, bool> onErrorOccured)
        {
            var exporter = new SimpleTextFileMazeExporter();

            exporter.UnexpectedValuesFound += onErrorOccured;

            var targetFileName = exporter.CreateTargetFileName(selectedMaze);

            var targetFile = new FileInfo(
                Path.Combine(pathToExportModelData, targetFileName));

            exporter.Export(selectedMaze, targetFile);

            SNEEDPreferences.DefaultExportDirectory = pathToExportModelData;
        }

        internal bool exportReady()
        {
           return Directory.Exists(pathToExportModelData) && selectedMaze != null;
        }

        internal void ChangeEditorModeTo(IEditorMode mode)
        {
            ResetCurrentEditorMode();

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
        internal void ResetCurrentEditorMode()
        {
            if (CurrentSelectedMode != null)
            {
                CurrentSelectedMode.Selected = false;
                CurrentSelectedMode.Reset();
                CurrentSelectedMode = null;
            }
        }
        
        [DrawGizmo(GizmoType.Active | GizmoType.InSelectionHierarchy, typeof(beMobileMaze))]
        static void DrawGizmosFor(beMobileMaze maze, GizmoType type)
        {
            var editorBackend = Instance;

            if (editorBackend == null)
                return;

            editorBackend.visual.pushHandleMatrix(maze.transform.localToWorldMatrix);

            editorBackend.visual.pushGizmoMatrix(maze.transform.localToWorldMatrix);

            editorBackend.visual.RenderTileHighlighting(maze.transform, maze.RoomDimension * 1.1f);
            

            var cam = Camera.current;

            if (GridEditingVisualUtils.CameraIsATopDownView(cam, maze.transform) && cam.orthographic)
            {
                schemaRenderer = new SchematicMazeRenderer();
                var offset = new Vector3(0, maze.RoomDimension.y + 1, 0);
                schemaRenderer.Render(maze, offset);
                schemaRenderer.RenderPaths(maze, offset);

            }


            if (editorBackend.CurrentSelectedMode != null)
            {

                editorBackend.CurrentSelectedMode.GizmoDrawings(maze, type);

            }

            editorBackend.visual.popGizmoMatrix();

            editorBackend.visual.popHandleMatrix();
        }
    }
}