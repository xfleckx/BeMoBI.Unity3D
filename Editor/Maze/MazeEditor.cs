using UnityEngine;
using UnityEditor;
using System;
using System.Text;
using Assets.SNEED.Mazes;

namespace Assets.SNEED.EditorExtensions.Maze
{
    public enum MazeEditorMode { NONE, EDITING, SELECTION }

    [CustomEditor(typeof(beMobileMaze))]
    public class MazeInspector : AMazeEditor
    {
        public const string STD_UNIT_PREFAB_NAME = "MazeUnit";

        private MazeUnit lastAddedUnit;

        public void OnEnable()
        {
            editorState = EditorState.Instance;
            
            var currentTarget = target as beMobileMaze;

            editorState.Initialize(currentTarget);

            if (editorState.SelectedMaze.Grid == null)
            {
                MazeEditorUtil.RebuildGrid(editorState.SelectedMaze);
            }

            editorState.referenceToPrefab = PrefabUtility.GetPrefabParent(editorState.SelectedMaze.gameObject);

            if (editorState.referenceToPrefab != null)
            {
                editorState.PathToMazePrefab = AssetDatabase.GetAssetPath(editorState.referenceToPrefab);
            }

            SetupGUIStyle();
            
        }

        public override void OnInspectorGUI()
        {
            if (editorState.SelectedMaze == null)
            {
                renderEmptyMazeGUI();
            }

            GUILayout.BeginVertical();

            GUILayout.Label("Properties", EditorStyles.boldLabel);

            EditorGUILayout.LabelField("Length of Maze (m)", editorState.SelectedMaze.MazeLengthInMeter.ToString());

            EditorGUILayout.LabelField("Width of Maze (m)", editorState.SelectedMaze.MazeWidthInMeter.ToString());

            EditorGUILayout.LabelField("Units:", editorState.SelectedMaze.Units.Count.ToString());

            EditorGUILayout.LabelField("Room size (m):", editorState.SelectedMaze.RoomDimension.ToString());

            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Open Customizer", GUILayout.MinWidth(200), GUILayout.Height(40)))
            {
                var window = CreateInstance<MazeCustomizer>();

                window.Initialize(editorState.SelectedMaze);

                window.Show();
            }

            if (GUILayout.Button("Open Editor", GUILayout.MinWidth(120), GUILayout.Height(40)))
            {
                var window = EditorWindow.GetWindow<MazeEditorWindow>();

                window.Initialize(editorState);

                window.Show();
            }

            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("Show Schema", GUILayout.Height(40)))
            {
                var sceneView = SceneView.lastActiveSceneView;
                var maze = editorState.SelectedMaze;

                sceneView.AlignViewToObject(maze.transform);
                var posAtMazeCenter = editorState.SelectedMaze.transform.position + new Vector3(maze.MazeLengthInMeter / 2, 0, maze.MazeWidthInMeter / 2);
                sceneView.LookAtDirect(posAtMazeCenter, Quaternion.Euler(90, 0, 0));
                sceneView.orthographic = true;

            }

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Close Maze Roof"))
            {
                foreach (var unit in editorState.SelectedMaze.Units)
                {
                    var topTransform = unit.transform.FindChild("Top");
                    if (topTransform != null)
                        topTransform.gameObject.SetActive(true);
                }
            }

            if (GUILayout.Button("Open Maze Roof"))
            {
                foreach (var unit in editorState.SelectedMaze.Units)
                {
                    var topTransform = unit.transform.FindChild("Top");
                    if (topTransform != null)
                        topTransform.gameObject.SetActive(false);
                }
            }

            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("Search for Units"))
            {
                MazeEditorUtil.CacheUnitsIn(editorState.SelectedMaze);
            }

            if (GUILayout.Button("Configure Grid"))
            {
                MazeEditorUtil.RebuildGrid(editorState.SelectedMaze);
            }

            if (editorState.referenceToPrefab && GUILayout.Button("Update Prefab"))
            {
                UpdatePrefabOfCurrentMaze();
            }

            if (GUILayout.Button("Repair Unit List"))
            {
                MazeEditorUtil.LookUpAllUnits(editorState.SelectedMaze);

            }

            GUILayout.EndVertical();

        }

        public void OnDisable()
        {
            if (editorState.SelectedMaze)
            {
                editorState.OnInspectorGetsDisabled();
            }
        }

        private void UpdatePrefabOfCurrentMaze()
        {
            editorState.referenceToPrefab = PrefabUtility.ReplacePrefab(editorState.SelectedMaze.gameObject, editorState.referenceToPrefab, ReplacePrefabOptions.ConnectToPrefab);

            EditorUtility.SetDirty(editorState.referenceToPrefab);
            EditorApplication.delayCall += AssetDatabase.SaveAssets;
        }

        private void SavePrefabAndCreateCompanionFolder()
        {
            editorState.PathToMazePrefab = EditorUtility.SaveFilePanelInProject("Save maze", "maze.prefab", "prefab", "Save maze as Prefab");
            Debug.Log("Saved to " + editorState.PathToMazePrefab);
            editorState.referenceToPrefab = PrefabUtility.CreatePrefab(editorState.PathToMazePrefab, editorState.SelectedMaze.gameObject, ReplacePrefabOptions.ConnectToPrefab);

            Debug.Log("Create companion folder " + editorState.PathToMazePrefab);
        }

        public override void RenderSceneViewUI()
        {
            if (EditorApplication.isPlaying)
                return;

            Handles.BeginGUI();

            GUILayout.BeginVertical(GUILayout.Width(200f));

            GUILayout.Label(string.Format("Row   : {0}", Math.Floor(editorState.currentTilePosition.y)));
            GUILayout.Label(string.Format("Column: {0}", Math.Floor(editorState.currentTilePosition.x)));

            GUILayout.Label("Position in local Space of the maze");
            GUILayout.Label(string.Format("{0} {1} {2}", this.editorState.mouseHitPos.x, this.editorState.mouseHitPos.y, this.editorState.mouseHitPos.z));
            GUILayout.Label(string.Format("Marker: {0} {1} {2}", editorState.MarkerPosition.x, editorState.MarkerPosition.y, editorState.MarkerPosition.z));
            
            GUILayout.EndVertical();

            Handles.EndGUI();
        }

        string RenderMazeGrid(beMobileMaze maze)
        {
            if (maze.Grid == null)
               return "No Grid available!";

            StringBuilder gridCode = new StringBuilder();
            StringBuilder line = new StringBuilder();

            int cols = maze.Grid.GetLength(0);
            int rows = maze.Grid.GetLength(1);

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    if (maze.Grid[c, r] != null)
                        line.AppendFormat(" {0}", 1);
                    else
                        line.AppendFormat(" {0}", 0);
                }

                gridCode.AppendLine(line.ToString());
                line.Remove(0, line.Length);
            };

            return gridCode.ToString();
        }

        private void renderEmptyMazeGUI()
        {
            GUILayout.BeginVertical();

            if (GUILayout.Button("Edit selected maze", GUILayout.Width(255)))
            {

            }

            GUILayout.EndVertical();
        }

    }
  
}