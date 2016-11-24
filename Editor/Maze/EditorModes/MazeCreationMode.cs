using UnityEngine;
using UnityEditor;
using Assets.SNEED.EditorExtensions.Maze.UnitCreation;
using Assets.SNEED.EditorExtensions;
using Assets.SNEED.Mazes;
using System;
using Assets.SNEED.EditorExtensions.Maze;

namespace Assets.SNEED.EditorExtension.Maze.EditorModes
{
    public class MazeCreationMode : EditorMode
    {
        private float MazeWidth;
        private float MazeLength;

        public override string Name
        {
            get
            {
               return "Maze Creation";
            }
        }

        public override Color GetPrimaryColor()
        {
            return Color.clear;
        }

        internal void CreateNewPlainMaze(MazeCreationWorkflowBackEnd backend)
        {
            backend.selectedMaze = new GameObject().AddComponent<beMobileMaze>();
            backend.selectedMaze.gameObject.AddComponent<PathController>();
        }

        public override void OnGUI(MazeCreationWorkflowBackEnd backend)
        {
            if(backend.selectedMaze == null && GUILayout.Button("New"))
            {
                CreateNewPlainMaze(backend);
            }

            if (backend.selectedMaze == null)
                return;

            var maze = backend.selectedMaze;

            maze.name = EditorGUILayout.TextField("Name", maze.name);

            maze.MazeWidthInMeter = EditorGUILayout.FloatField("Width", maze.MazeWidthInMeter);

            maze.MazeLengthInMeter = EditorGUILayout.FloatField("Length", maze.MazeLengthInMeter);
            
            maze.RoomDimension = EditorGUILayout.Vector3Field("Room WxHxL (m):", maze.RoomDimension, null);

            var gridSize = beMobileMaze.CalcGridSize(maze);

            MazeEditorUtil.RebuildGrid(maze);

            EditorGUILayout.Vector2Field("Grid Cols(x) Rows(y):", gridSize, null);

            if (GUILayout.Button("Create"))
            {
                backend.selectedMaze = maze;
            }

        }

        public override void OnSceneViewGUI(SceneView view, MazeCreationWorkflowBackEnd backend, EditorViewGridVisualisation visual)
        {
            base.OnSceneViewGUI(view, backend, visual);

            if (backend.selectedMaze != null)
                EditorVisualUtils.HandlesDrawGrid(backend.selectedMaze);
        }
        public override void Reset()
        { 


        }

        protected override void Click(Event evt, int button)
        {
            // does nothing
        }

        protected override void Drag(Event evt, int button)
        {
            // todo maybe scaling the maze grid
        }

    }
}