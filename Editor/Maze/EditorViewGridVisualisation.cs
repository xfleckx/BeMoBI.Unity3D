using UnityEngine;
using UnityEditor;
using Assets.SNEED.EditorExtensions.Maze;
using Assets.SNEED.Mazes;
using System.Linq;
using System.Collections.Generic;
using System;

namespace Assets.SNEED.EditorExtensions
{
    public class EditorViewGridVisualisation
    {
        public Vector2 currentTilePosition;
        public Vector3 lastMouseHit;
        public Vector3 MarkerPosition;

        private Stack<Color> handleColors = new Stack<Color>();

        private Stack<Matrix4x4> handleMatrix = new Stack<Matrix4x4>();

        public Color CurrentHighlightingColor { get; internal set; }

        public void pushHandleColor()
        {
            handleColors.Push(Handles.color);
        }

        public void  popHandleColor()
        {
            if (!handleColors.Any())
                return;

            Handles.color = handleColors.Pop();
        }

        public void pushHandleMatrix()
        {
            handleMatrix.Push(Handles.matrix);
        }

        public void popHandleMatrix()
        {
            if (!handleMatrix.Any())
                return;

            Handles.matrix = handleMatrix.Pop();
        }

        internal void CalculateTilePosition(Transform target, Vector3 tileSize, int rows, int columns)
        {
            // if UpdateHitPosition return true we should update the scene views so that the marker will update in real time
            if (GridEditingVisualUtils.UpdateHitPosition(target, ref lastMouseHit))
            {
                // set the TileMap.MarkerPosition value
                currentTilePosition = GridEditingVisualUtils.GetTilePositionFromMouseLocation(target, tileSize, lastMouseHit, new Vector2(columns, rows));

                currentTilePosition = GridEditingVisualUtils.ConstrainTilePosition(currentTilePosition, rows, columns);

                MarkerPosition = GridEditingVisualUtils.RecalculateMarkerPosition(currentTilePosition, tileSize);

                //SceneView.currentDrawingSceneView.Repaint();
            }
        }

        internal void RenderTileHighlighting(Transform target, Vector3 size)
        {
            var pos = MarkerPosition + new Vector3(0, size.y / 2, 0);
            pushHandleMatrix();

            pushHandleColor();

            Handles.matrix = target.localToWorldMatrix;

            Handles.color = CurrentHighlightingColor;

            Handles.DrawWireCube(pos, size);

            Handles.Label(pos, string.Format("{0}.{1}", currentTilePosition.x, currentTilePosition.y));

            popHandleColor();

            popHandleMatrix();
        }

        internal void renderDebugInfos()
        {
            Handles.BeginGUI();

            GUILayout.BeginVertical(GUILayout.Width(200f));
            
            GUILayout.Label("Position in local Space of the maze");
            GUILayout.Label(string.Format("{0} {1} {2}", lastMouseHit.x, lastMouseHit.y, lastMouseHit.z));
            GUILayout.Label(string.Format("Marker: {0} {1} {2}", MarkerPosition.x, MarkerPosition.y, MarkerPosition.z));

            GUILayout.Label(string.Format("Col  : {0}", currentTilePosition.x));
            GUILayout.Label(string.Format("Row : {0}", currentTilePosition.y));

            GUILayout.EndVertical();

            Handles.EndGUI();
        }

        public void RenderEditorGizmos(beMobileMaze maze)
        {

            var tempMatrix = Gizmos.matrix;

            Gizmos.matrix = maze.transform.localToWorldMatrix;

            var temp = Handles.matrix;
            Handles.matrix = Gizmos.matrix;

            //if (editorState.EditorWindowVisible || !maze.Units.Any())
            //    EditorVisualUtils.DrawFloorGrid(maze);

            Gizmos.color = Color.blue;

            // this is a custom  feature for selecting multiple MazeUnits
            //if (editorState.EditorWindowVisible && editorState.CurrentSelection != null)
            //{
            //    foreach (var item in editorState.CurrentSelection)
            //    {
            //        var pos = item.transform.localPosition + new Vector3(0, maze.RoomDimension.y / 2, 0);
            //        Gizmos.DrawCube(pos, new Vector3(maze.RoomDimension.x, maze.RoomDimension.y, maze.RoomDimension.z));
            //    }
            //}

            Handles.matrix = temp;
            Gizmos.matrix = tempMatrix;
        }
    }
}
