using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.SNEED.EditorExtensions.Maze
{
    public class SchematicMazeRenderer
    {
        public void Render(beMobileMaze maze, Vector3 drawingOffset)
        {
            var schemaWallWidth = 0.1f;
            var schemaWallLength = maze.RoomDimension.x;

            var temp = Gizmos.color;
            var schematicUnitSize = new Vector3(maze.RoomDimension.x, 0, maze.RoomDimension.z);

            foreach (var unit in maze.Units)
            {
                Gizmos.color = Color.white;
                Gizmos.DrawCube(unit.transform.position, schematicUnitSize);

                Gizmos.color = Color.black;
                foreach (var child in unit.transform.AllChildren())
                {
                    if (!child.activeSelf)
                        continue;

                    if (child.name == "Top")
                        continue;

                    var position = child.transform.position;

                    if (child.name == "West" || child.name == "East")
                    {
                        var wallDimensions = new Vector3(schemaWallWidth, 0, schemaWallLength);
                        Gizmos.DrawCube(position + drawingOffset, wallDimensions);
                    }


                    if (child.name == "North" || child.name == "South")
                    {
                        var wallDimensions = new Vector3(schemaWallLength, 0, schemaWallWidth);
                        Gizmos.DrawCube(position + drawingOffset, wallDimensions);
                    }
                }
            }

            Gizmos.color = temp;
        }

        public void RenderPaths(beMobileMaze maze, Vector3 drawingOffset)
        {
            var pathController = maze.GetComponent<PathController>();

            if (pathController == null)
                return;

            foreach (var path in pathController.Paths)
            {
                PathEditorUtils.RenderPathElements(maze, path, drawingOffset);
            }

        }
    }
}
