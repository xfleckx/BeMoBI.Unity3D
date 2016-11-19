using UnityEditor;
using UnityEngine;
using Assets.SNEED.EditorExtensions.Util;
using Assets.SNEED.Mazes;

namespace Assets.SNEED.EditorExtensions.Maze
{
    public static class EditorVisualUtils
    {
        public static void DrawFloorGrid(beMobileMaze maze)
        {
            // store map width, height and position
            var mapWidth = maze.MazeWidthInMeter + maze.RoomDimension.x;
            var mapHeight = maze.MazeLengthInMeter + maze.RoomDimension.z;

            //var position = maze.transform.position;
            var position = new Vector3(0, 0, 0);

            var ro_x1 = new Vector3(-0.01f, 0, -0.01f);
            var ro_x2 = new Vector3(0.01f, 0, -0.01f);
            var ro_x3 = new Vector3(-0.01f, 0, 0.01f);
            var ro_x4 = new Vector3(0.01f, 0, 0.01f);
            // draw layer border
            Gizmos.color = Color.black;
            Gizmos.DrawLine(position + ro_x1, position + new Vector3(mapWidth, 0, 0) + ro_x2);
            Gizmos.DrawLine(position + ro_x1, position + new Vector3(0, 0, mapHeight) + ro_x3);
            Gizmos.DrawLine(position + new Vector3(mapWidth, 0, 0) + ro_x2, position + new Vector3(mapWidth, 0, mapHeight) + ro_x4);
            Gizmos.DrawLine(position + new Vector3(0, 0, mapHeight) + ro_x3 , position + new Vector3(mapWidth, 0, mapHeight) + ro_x4);

            Vector3 lineStart;
            Vector3 lineEnde;
            // draw tile cells
            Gizmos.color = Color.white * new Color(1,1,1,0.5f);

            for (float i = 0; i <= maze.Columns; i++)
            {
                float xOffset = i * maze.RoomDimension.x;

                lineStart = position + new Vector3(xOffset, 0, 0);
                lineEnde = position + new Vector3(xOffset, 0, mapHeight);

                var labelPos = position + new Vector3(xOffset, 0, 0);
                Handles.Label(labelPos, i.ToString());

                Gizmos.DrawLine(lineStart, lineEnde);
            }

            for (float i = 0; i <= maze.Rows; i++)
            {
                float yoffset = i * maze.RoomDimension.z;

                lineStart = position + new Vector3(0, 0, yoffset);
                lineEnde = position + new Vector3(mapWidth, 0, yoffset);

                var labelPos = position + new Vector3(0, 0, yoffset);
                Handles.Label(labelPos, i.ToString());

                Gizmos.DrawLine(lineStart, lineEnde);
            }

            var zeroField = new Vector3(position.x + (maze.RoomDimension.x / 2), 0, position.x + (maze.RoomDimension.x / 2));

            Gizmos.color = new Color(Color.green.r, Color.green.g, Color.green.b, 0.1f);

            Gizmos.DrawCube(zeroField, new Vector3(maze.RoomDimension.x - 0.1f, 0, maze.RoomDimension.z - 0.1f));
        }

        public static void HandlesDrawGrid(beMobileMaze maze)
        {
            // store map width, height and position
            var mapWidth = maze.MazeWidthInMeter + maze.RoomDimension.x;
            var mapHeight = maze.MazeLengthInMeter + maze.RoomDimension.z;

            //var position = maze.transform.position;
            var position = new Vector3(0, 0, 0);

            var ro_x1 = new Vector3(-0.01f, 0, -0.01f);
            var ro_x2 = new Vector3(0.01f, 0, -0.01f);
            var ro_x3 = new Vector3(-0.01f, 0, 0.01f);
            var ro_x4 = new Vector3(0.01f, 0, 0.01f);
            // draw layer border
            Handles.color = Color.black;
            Handles.DrawLine(position + ro_x1, position + new Vector3(mapWidth, 0, 0) + ro_x2);
            Handles.DrawLine(position + ro_x1, position + new Vector3(0, 0, mapHeight) + ro_x3);
            Handles.DrawLine(position + new Vector3(mapWidth, 0, 0) + ro_x2, position + new Vector3(mapWidth, 0, mapHeight) + ro_x4);
            Handles.DrawLine(position + new Vector3(0, 0, mapHeight) + ro_x3, position + new Vector3(mapWidth, 0, mapHeight) + ro_x4);

            Vector3 lineStart;
            Vector3 lineEnde;
            // draw tile cells
            Handles.color = Color.white * new Color(1, 1, 1, 0.5f);

            for (float i = 0; i <= maze.Columns; i++)
            {
                float xOffset = i * maze.RoomDimension.x;

                lineStart = position + new Vector3(xOffset, 0, 0);
                lineEnde = position + new Vector3(xOffset, 0, mapHeight);

                var labelPos = position + new Vector3(xOffset, 0, 0);
                Handles.Label(labelPos, i.ToString());

                Handles.DrawLine(lineStart, lineEnde);
            }

            for (float i = 0; i <= maze.Rows; i++)
            {
                float yoffset = i * maze.RoomDimension.z;

                lineStart = position + new Vector3(0, 0, yoffset);
                lineEnde = position + new Vector3(mapWidth, 0, yoffset);

                var labelPos = position + new Vector3(0, 0, yoffset);
                Handles.Label(labelPos, i.ToString());

                Handles.DrawLine(lineStart, lineEnde);
            }

            var zeroField = new Vector3(position.x + (maze.RoomDimension.x / 2), 0, position.x + (maze.RoomDimension.x / 2));

            Handles.color = new Color(Color.green.r, Color.green.g, Color.green.b, 0.1f);

            Handles.RectangleCap(-1, zeroField, Quaternion.identity, 0.5f);
        }
    }
    
}
