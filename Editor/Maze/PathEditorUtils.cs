using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Assets.SNEED.EditorExtensions.Maze
{
    public static class PathEditorUtils
    {
        public static void RenderPathElements(beMobileMaze maze, PathInMaze instance, Vector3 drawingOffset)
        {
            var temp_handles_color = Handles.color;

            if (instance.PathAsLinkedList.Count > 0)
            {

                var start = instance.PathAsLinkedList.First.Value.Unit.transform;

                Handles.color = Color.blue;

                Handles.CubeCap(0, start.position + drawingOffset, start.rotation, 0.3f);


                var iterator = instance.PathAsLinkedList.GetEnumerator();
                MazeUnit last = null;

                while (iterator.MoveNext())
                {
                    if (last == null)
                    {
                        last = iterator.Current.Unit;
                        continue;
                    }

                    if (last == null || iterator.Current.Unit == null)
                    {
                        last = iterator.Current.Unit;
                        continue;
                    }

                    Gizmos.DrawLine(last.transform.position + drawingOffset, iterator.Current.Unit.transform.position + drawingOffset);

                    last = iterator.Current.Unit;
                }

                var lastElement = instance.PathAsLinkedList.Last.Value.Unit;
                var endTransform = lastElement.transform;

                var coneRotation = start.rotation;

                switch (lastElement.WaysOpen)
                {
                    case OpenDirections.None:
                        break;
                    case OpenDirections.North:
                        coneRotation.SetLookRotation(-endTransform.forward);
                        break;
                    case OpenDirections.South:
                        coneRotation.SetLookRotation(endTransform.forward);
                        break;
                    case OpenDirections.East:
                        coneRotation.SetLookRotation(-endTransform.right);
                        break;
                    case OpenDirections.West:
                        coneRotation.SetLookRotation(endTransform.right);
                        break;
                    case OpenDirections.All:
                        break;
                    default:
                        break;
                }

                Handles.ConeCap(0, endTransform.position + drawingOffset, coneRotation, 0.3f);
                Handles.color = temp_handles_color;
            }
        }
    }
}
