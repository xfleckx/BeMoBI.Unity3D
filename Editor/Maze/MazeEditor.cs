using UnityEngine;
using UnityEditor;
using System;
using System.Text;
using Assets.SNEED.Mazes;
using Assets.SNEED.EditorExtension.Maze;

namespace Assets.SNEED.EditorExtensions.Maze
{
    public enum MazeEditorMode { NONE, EDITING, SELECTION }

    [CustomEditor(typeof(beMobileMaze))]
    public class MazeInspector : Editor 
    {
        public override void OnInspectorGUI()
        {
            var maze = target as beMobileMaze;

            GUILayout.BeginVertical();

            GUILayout.Label("Properties", EditorStyles.boldLabel);

            EditorGUILayout.LabelField("Length of Maze (m)", maze.MazeLengthInMeter.ToString());

            EditorGUILayout.LabelField("Width of Maze (m)", maze.MazeWidthInMeter.ToString());

            EditorGUILayout.LabelField("Units:", maze.Units.Count.ToString());

            EditorGUILayout.LabelField("Room size (m):", maze.RoomDimension.ToString());

            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Open Customizer", GUILayout.MinWidth(200), GUILayout.Height(40)))
            {
                var window = CreateInstance<MazeCustomizer>();

                window.Initialize(maze);

                window.Show();
            }

            if (GUILayout.Button("Open Editor", GUILayout.MinWidth(120), GUILayout.Height(40)))
            {
                var window = EditorWindow.GetWindow<MazeCreationWorkflow>();
                window.Show();
            }

            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("Show Schema", GUILayout.Height(40)))
            {
                var sceneView = SceneView.lastActiveSceneView; 

                sceneView.AlignViewToObject(maze.transform);
                var posAtMazeCenter = maze.transform.position + new Vector3(maze.MazeLengthInMeter / 2, 0, maze.MazeWidthInMeter / 2);
                sceneView.LookAtDirect(posAtMazeCenter, Quaternion.Euler(90, 0, 0));
                sceneView.orthographic = true;

            }
            
            GUILayout.EndVertical();

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
        

    }
  
}