using Assets.SNEED.Mazes;
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
        public static void RenderPathElements(beMobileMaze maze, PathInMaze instance, Vector3 drawingOffset, Color color)
        {
            var tempGizmoColor = Gizmos.color;

            if (instance.PathAsLinkedList.Count > 0)
            {

                var start = instance.PathAsLinkedList.First.Value.Unit.transform;
                
                Gizmos.color = color;

                Gizmos.DrawCube(start.position + drawingOffset, Vector3.one * 0.3f);

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
                Gizmos.DrawSphere(endTransform.position + drawingOffset, 0.15f);

                Gizmos.color = tempGizmoColor;
            }
        }
    }
}
