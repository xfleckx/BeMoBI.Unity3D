using UnityEngine;
using System.Collections;
using UnityEditor;
using System;
using System.Linq;

namespace Assets.SNEED.EditorExtensions.Maze
{
    /// <summary>
    /// A abstract base class extending the default editor with abilities to render and interact a grid in the SceneView
    /// </summary>
    public abstract class AMazeEditor : Editor
    {
        private static SchematicMazeRenderer schemaRenderer;

        protected EditorState editorState;

        public static GUIStyle sceneViewUIStyle;

        public void OnSceneGUI()
        {
            if (!editorState.EditorWindowVisible)
                return;

            TileHighlightingOnMouseCursor();

            RenderSceneViewUI();

            if (editorState.EditorWindowVisible && editorState.EditorModeProcessEvent != null)
                editorState.EditorModeProcessEvent(Event.current);
        }

        protected void TileHighlightingOnMouseCursor()
        {
            // if UpdateHitPosition return true we should update the scene views so that the marker will update in real time
            if (UpdateHitPosition(editorState.SelectedMaze.transform))
            {
                // set the TileMap.MarkerPosition value
                editorState.MarkerPosition = RecalculateMarkerPosition();

                editorState.currentTilePosition = GetTilePositionFromMouseLocation(editorState.SelectedMaze, editorState.mouseHitPos);

                SceneView.currentDrawingSceneView.Repaint();
            }

        }

        public abstract void RenderSceneViewUI();

        protected virtual void SetupGUIStyle()
        {
            sceneViewUIStyle = new GUIStyle();
            sceneViewUIStyle.normal.textColor = Color.blue;
        }

        [DrawGizmo(GizmoType.Active | GizmoType.InSelectionHierarchy, typeof(beMobileMaze))]
        static void DrawGizmosFor(beMobileMaze maze, GizmoType type)
        {
            var editorState = EditorState.Instance;

            RenderEditorGizmos(maze, editorState);
            RenderTileHighlighting(maze, editorState, AMazeEditor.sceneViewUIStyle);

        }

        private static void RenderTileHighlighting(beMobileMaze maze, EditorState editorState, GUIStyle sceneViewUIStyle)
        {
            if (!editorState.EditorWindowVisible)
                return;

            var tempMatrix = Gizmos.matrix;

            Gizmos.matrix = maze.transform.localToWorldMatrix;

            Gizmos.color = editorState.MarkerColor;

            var pos = editorState.MarkerPosition + new Vector3(0, maze.RoomDimension.y / 2, 0);

            Gizmos.DrawWireCube(pos, new Vector3(maze.RoomDimension.x, maze.RoomDimension.y, maze.RoomDimension.z) * 1.1f);

            var temp = Handles.matrix;

            Handles.matrix = Gizmos.matrix;

            Handles.Label(pos, string.Format("{0}.{1}", (int)editorState.MarkerPosition.x, (int)editorState.MarkerPosition.z), sceneViewUIStyle);

            Handles.matrix = temp;

            Gizmos.matrix = tempMatrix;
        }
        
        private static void RenderEditorGizmos(beMobileMaze maze, EditorState editorState)
        {
            var cam = Camera.current;

            if (cameraIsATopDownView(cam, maze) && cam.orthographic)
            {
                if (schemaRenderer == null)
                {
                    schemaRenderer = new SchematicMazeRenderer();
                }

                var drawingOffset = new Vector3(0, maze.RoomDimension.y + 0.5f, 0);
                schemaRenderer.Render(maze, drawingOffset);
                schemaRenderer.RenderPaths(maze, drawingOffset);

            }

            var tempMatrix = Gizmos.matrix;

            Gizmos.matrix = maze.transform.localToWorldMatrix;

            var temp = Handles.matrix;
            Handles.matrix = Gizmos.matrix;

            if (editorState.EditorWindowVisible || !maze.Units.Any())
                EditorVisualUtils.DrawFloorGrid(editorState.SelectedMaze);

            Gizmos.color = Color.blue;

            if (editorState.EditorWindowVisible && editorState.CurrentSelection != null)
            {
                foreach (var item in editorState.CurrentSelection)
                {
                    var pos = item.transform.localPosition + new Vector3(0, maze.RoomDimension.y / 2, 0);
                    Gizmos.DrawCube(pos, new Vector3(maze.RoomDimension.x, maze.RoomDimension.y, maze.RoomDimension.z));
                }
            }

            Handles.matrix = temp;
            Gizmos.matrix = tempMatrix;
        }

        private static bool cameraIsATopDownView(Camera cam, beMobileMaze maze)
        {
            var currentCamForward = cam.transform.TransformVector(cam.transform.forward);

            var currentMazeDown = maze.transform.TransformVector(-maze.transform.forward);

            return currentCamForward == currentMazeDown;
        }

        #region General calculations based on tile editor

        /// <summary>
        /// Calculates the position of the mouse over the tile maze in local space coordinates.
        /// </summary>
        /// <returns>Returns true if the mouse is over the tile maze.</returns>
        protected bool UpdateHitPosition(Transform targetLocalSpace)
        {
            // build a plane object that 
            var p = new Plane(targetLocalSpace.TransformDirection(targetLocalSpace.up), targetLocalSpace.position);

            // build a ray type from the current mouse position
            var ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);

            // stores the hit location
            var hit = new Vector3();

            // stores the distance to the hit location
            float dist;

            // cast a ray to determine what location it intersects with the plane
            if (p.Raycast(ray, out dist))
            {
                // the ray hits the plane so we calculate the hit location in world space
                hit = ray.origin + (ray.direction.normalized * dist);
            }

            // convert the hit location from world space to local space
            var value = targetLocalSpace.InverseTransformPoint(hit);

            // if the value is different then the current mouse hit location set the 
            // new mouse hit location and return true indicating a successful hit test
            if (value != editorState.mouseHitPos)
            {
                editorState.mouseHitPos = value;
                return true;
            }

            // return false if the hit test failed
            return false;
        }

        /// <summary>
        /// Calculates the location in tile coordinates (Column/Row) of the mouse position
        /// </summary>
        /// <returns>Returns a <see cref="Vector2"/> type representing the Column and Row where the mouse of positioned over.</returns>
        protected Vector2 GetTilePositionFromMouseLocation(beMobileMaze maze, Vector3 mouseHit)
        {
            // calculate column and row location from mouse hit location
            var pos = new Vector3(
               (int) mouseHit.x / maze.RoomDimension.x,
               (int) mouseHit.y / maze.transform.position.y,
               (int) mouseHit.z / maze.RoomDimension.z);


            // round the numbers to the nearest whole number using 5 decimal place precision
            //pos = new Vector3(
            //    (int)Math.Round(pos.x, 5, MidpointRounding.ToEven),
            //    (int)Math.Round(pos.y, 5, MidpointRounding.ToEven),
            //    (int)Math.Round(pos.z, 5, MidpointRounding.ToEven));

            pos = new Vector3((int)pos.x, (int)pos.y, (int)pos.z);

            // do a check to ensure that the row and column are with the bounds of the tile maze
            var col = pos.x;
            var row = pos.z;

            if (row < 0)
            {
                row = 0;
            }

            if (row > maze.Rows - 1)
            {
                row = maze.Rows - 1;
            }

            if (col < 0)
            {
                col = 0;
            }

            if (col > maze.Columns - 1)
            {
                col = maze.Columns - 1;
            }

            // return the column and row values
            return new Vector2(col, row);
        }

        /// <summary>
        /// Recalculates the position of the marker based on the location of the mouse pointer.
        /// </summary>
        protected Vector3 RecalculateMarkerPosition()
        {
            // store the tile position in world space
            var pos = new Vector3(
                editorState.currentTilePosition.x * editorState.SelectedMaze.RoomDimension.x,
                0,
                editorState.currentTilePosition.y * editorState.SelectedMaze.RoomDimension.z);

            return new Vector3(
                pos.x + (editorState.SelectedMaze.RoomDimension.x / 2),
                pos.y,
                pos.z + (editorState.SelectedMaze.RoomDimension.z / 2));
        }

        #endregion

    }
}