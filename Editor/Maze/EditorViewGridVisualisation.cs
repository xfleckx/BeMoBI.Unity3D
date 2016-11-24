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

        private Stack<Matrix4x4> handleMatrices = new Stack<Matrix4x4>();

        private Stack<Matrix4x4> gizmoMatrices = new Stack<Matrix4x4>();

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

        public void pushHandleMatrix(Matrix4x4 localToWorldMatrix)
        {
            handleMatrices.Push(Handles.matrix);
            Handles.matrix = localToWorldMatrix;
        }

        public void popHandleMatrix()
        {
            if (!handleMatrices.Any())
                return;

            Handles.matrix = handleMatrices.Pop();
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
                
            }
        }

        internal void RenderTileHighlighting(Transform target, Vector3 size)
        {
            var pos = MarkerPosition + new Vector3(0, size.y / 2, 0);

            Handles.color = CurrentHighlightingColor;

            Handles.DrawWireCube(pos, size);

            Handles.Label(pos, string.Format("{0}.{1}", currentTilePosition.x, currentTilePosition.y)); 
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

        internal void pushGizmoMatrix(Matrix4x4 localToWorldMatrix)
        {
            gizmoMatrices.Push(Gizmos.matrix);
            Gizmos.matrix = localToWorldMatrix;
        }

        internal void popGizmoMatrix()
        {
            if (!gizmoMatrices.Any())
                return;

            Gizmos.matrix = gizmoMatrices.Pop();
        }
    }
}
