using UnityEngine;
using UnityEditor;
using Assets.SNEED.EditorExtensions.Maze.UnitCreation;
using Assets.SNEED.EditorExtensions;
using Assets.SNEED.Mazes;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Assets.SNEED.EditorExtension.Maze.EditorModes
{
    public class MergeAndSeparateMode : EditorMode
    {
        public List<GameObject> CurrentSelection = new List<GameObject>();

        public override string Name
        {
            get
            {
                return "Connect Units";
            }
        }

        public override Color GetPrimaryColor()
        {
            return Color.blue;
        }

        public override void Reset()
        {
            CurrentSelection.Clear();
        }
        
        protected override void Click(Event evt, int button)
        {
            var pos = backend.visual.currentTilePosition;

            var unitHost = GameObject.Find(string.Format(backend.selectedMaze.UnitNamePattern, pos.x, pos.y));

            if (unitHost != null)
            {
                if (CurrentSelection.Contains(unitHost))
                {
                    if (evt.button == 1)
                        CurrentSelection.Remove(unitHost);
                }
                else
                {
                    if (evt.button == 0)
                        CurrentSelection.Add(unitHost);
                }


            }

            Consume(evt);
        }

        public override void OnGUI(MazeCreationWorkflowBackEnd backend)
        {
            if(!CurrentSelection.Any())
                EditorGUILayout.HelpBox("Select Units first", MessageType.None);

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Remove Walls"))
            {
                TryConnectingCurrentSelection();
            }

            if (GUILayout.Button("Add Walls"))
            {
                TryDisconnectingCurrentSelection();
            }
            
            EditorGUILayout.EndHorizontal();
        }

        public override void OnSceneViewGUI(SceneView view, MazeCreationWorkflowBackEnd backend, EditorViewGridVisualisation visual)
        {
            base.OnSceneViewGUI(view, backend, visual);
        }

        public override void GizmoDrawings(beMobileMaze maze, GizmoType type)
        {
            base.GizmoDrawings(maze, type);

            var temp = Gizmos.color;

            Gizmos.color = GetPrimaryColor() * new Color(1,1,1,0.4f);

            foreach (var item in CurrentSelection)
            {
                var pos = item.transform.localPosition + new Vector3(0, maze.RoomDimension.y / 2, 0);
                Gizmos.DrawCube(pos, new Vector3(maze.RoomDimension.x, maze.RoomDimension.y, maze.RoomDimension.z));
            }

            Gizmos.color = temp;
        }

        protected override void Drag(Event evt, int button)
        {
        }


        #region logic
        public void TryConnectingCurrentSelection()
        {
            if (CurrentSelection == null)
                return;

            if (!CurrentSelection.Any())
                return;

            var iterator = CurrentSelection.GetEnumerator();

            MazeUnit last = null;

            while (iterator.MoveNext())
            {
                var current = iterator.Current.GetComponent<MazeUnit>();

                if (!last)
                {
                    last = current;
                    continue;
                }
                Debug.Log(current.GridID.ToString());
                // check if current and last are really neighbors:
                if (Math.Abs(current.GridID.x - last.GridID.x) + Math.Abs(current.GridID.y - last.GridID.y) == 1)
                {
                    // check which direction we go, possibilities:
                    if (current.GridID.x - last.GridID.x == 1) // going east
                    {
                        last.Open(OpenDirections.East);
                        current.Open(OpenDirections.West);
                    }
                    else if (current.GridID.x - last.GridID.x == -1) // going west
                    {
                        last.Open(OpenDirections.West);
                        current.Open(OpenDirections.East);
                    }

                    if (current.GridID.y - last.GridID.y == 1) // going north
                    {
                        last.Open(OpenDirections.North);
                        current.Open(OpenDirections.South);
                    }
                    else if (current.GridID.y - last.GridID.y == -1) // going south
                    {
                        last.Open(OpenDirections.South);
                        current.Open(OpenDirections.North);
                    }
                }


                last = current;
            }
        }

        public void TryDisconnectingCurrentSelection()
        {
            if (CurrentSelection == null)
                return;

            if (!CurrentSelection.Any())
                return;


            if (CurrentSelection.Count == 1)
            {
                var unit = CurrentSelection.First().GetComponent<MazeUnit>();
                unit.Close(OpenDirections.North);
                unit.Close(OpenDirections.South);
                unit.Close(OpenDirections.West);
                unit.Close(OpenDirections.East);
            }

            var iterator = CurrentSelection.GetEnumerator();

            MazeUnit last = null;

            while (iterator.MoveNext())
            {
                var current = iterator.Current.GetComponent<MazeUnit>();

                if (!last)
                {
                    last = current;
                    continue;
                }

                if (current.GridID.x - 1 == last.GridID.x)
                {
                    last.Close(OpenDirections.East);
                    current.Close(OpenDirections.West);
                }
                else if (current.GridID.x + 1 == last.GridID.x)
                {
                    last.Close(OpenDirections.West);
                    current.Close(OpenDirections.East);
                }

                if (current.GridID.y - 1 == last.GridID.y)
                {
                    last.Close(OpenDirections.North);
                    current.Close(OpenDirections.South);
                }
                else if (current.GridID.y + 1 == last.GridID.y)
                {
                    last.Close(OpenDirections.South);
                    current.Close(OpenDirections.North);
                }

                last = current;
            }
        }
        
        #endregion
    }


}