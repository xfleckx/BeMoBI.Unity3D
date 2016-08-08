using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Assets.SNEED.EditorExtensions.Grid
{
    [CustomEditor(typeof(RectangularGrid))]
    public class RectangularGridInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
        }

        [DrawGizmo(GizmoType.Selected | GizmoType.Active)]
        static void DrawGridGizmo(RectangularGrid grid, GizmoType gizmoType)
        {
            // store map width, height and position
            var mapWidth = grid.width;
            var mapLength = grid.height;

            var columns = mapWidth / grid.fieldWidth;
            var rows = mapLength / grid.fieldLength;

            var position = new Vector3(0, 0, 0);

            // draw layer border
            Gizmos.color = Color.white;
            Gizmos.DrawLine(position, position + new Vector3(mapWidth, 0, 0));
            Gizmos.DrawLine(position, position + new Vector3(0, 0, mapLength));
            Gizmos.DrawLine(position + new Vector3(mapWidth, 0, 0), position + new Vector3(mapWidth, 0, mapLength));
            Gizmos.DrawLine(position + new Vector3(0, 0, mapLength), position + new Vector3(mapWidth, 0, mapLength));

            Vector3 lineStart;
            Vector3 lineEnde;
            // draw tile cells
            Gizmos.color = Color.green;

            for (float i = 1; i <= columns; i++)
            {
                float xOffset = i * grid.fieldWidth;

                lineStart = position + new Vector3(xOffset, 0, 0);
                lineEnde = position + new Vector3(xOffset, 0, mapLength);

                var labelPos = position + new Vector3(xOffset, 0, 0);
                Handles.Label(labelPos, i.ToString());

                Gizmos.DrawLine(lineStart, lineEnde);
            }

            for (float i = 1; i <= rows; i++)
            {
                float yoffset = i * grid.fieldLength;

                lineStart = position + new Vector3(0, 0, yoffset);
                lineEnde = position + new Vector3(mapWidth, 0, yoffset);

                var labelPos = position + new Vector3(0, 0, yoffset);
                Handles.Label(labelPos, i.ToString());

                Gizmos.DrawLine(lineStart, lineEnde);
            }

            var zeroField = new Vector3(position.x + (grid.fieldWidth / 2), 0, position.x + (grid.fieldWidth / 2));

            Gizmos.color = new Color(Color.green.r, Color.green.g, Color.green.b, 0.1f);

            Gizmos.DrawCube(zeroField, new Vector3(grid.fieldWidth - 0.1f, 0, grid.fieldLength - 0.1f));
        }
    }
}
