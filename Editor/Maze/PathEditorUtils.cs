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

        public static PathElement GetElementType(PathElement element)
        {
            var u = element.Unit;

            if (u.WaysOpen == (OpenDirections.East | OpenDirections.West) ||
                u.WaysOpen == (OpenDirections.North | OpenDirections.South) ||
                u.WaysOpen == OpenDirections.East ||
                u.WaysOpen == OpenDirections.West ||
                u.WaysOpen == OpenDirections.North ||
                u.WaysOpen == OpenDirections.South)
            {
                element.Type = UnitType.I;
            }

            if (u.WaysOpen == OpenDirections.All)
                element.Type = UnitType.X;

            if (u.WaysOpen == (OpenDirections.West | OpenDirections.North | OpenDirections.East) ||
               u.WaysOpen == (OpenDirections.West | OpenDirections.South | OpenDirections.East) ||
               u.WaysOpen == (OpenDirections.West | OpenDirections.South | OpenDirections.North) ||
                u.WaysOpen == (OpenDirections.East | OpenDirections.South | OpenDirections.North))
            {
                element.Type = UnitType.T;
            }

            if (u.WaysOpen == (OpenDirections.West | OpenDirections.North) ||
               u.WaysOpen == (OpenDirections.West | OpenDirections.South) ||
               u.WaysOpen == (OpenDirections.East | OpenDirections.South) ||
                u.WaysOpen == (OpenDirections.East | OpenDirections.North))
            {
                element.Type = UnitType.L;
            }

            return element;
        }

        public static PathElement GetTurnType(PathElement current, PathElement last, PathElement sec2last)
        {
            var x0 = sec2last.Unit.GridID.x;
            var y0 = sec2last.Unit.GridID.y;
            //  var x1 = last.Unit.GridID.x; // why unused?
            var y1 = last.Unit.GridID.y;
            var x2 = current.Unit.GridID.x;
            var y2 = current.Unit.GridID.y;

            if ((x0 - x2) - (y0 - y2) == 0) // same sign
            {
                if (y0 != y1) // first change in y
                {
                    last.Turn = TurnType.RIGHT;
                }
                else
                {
                    last.Turn = TurnType.LEFT;
                }
            }
            else // different sign
            {
                if (y0 != y1) // first change in y
                {
                    last.Turn = TurnType.LEFT;
                }
                else
                {
                    last.Turn = TurnType.RIGHT;
                }
            }

            if (Math.Abs(x0 - x2) == 2 || Math.Abs(y0 - y2) == 2)
            {
                last.Turn = TurnType.STRAIGHT;
            }

            return current;
        }

        public static void RenderPathElements(beMobileMaze maze, PathInMaze instance, Vector3 drawingOffset, Color color)
        {
            var tempGizmoColor = Gizmos.color;

            if (instance.PathAsLinkedList != null && instance.PathAsLinkedList.Count > 0)
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
