using UnityEngine;
using UnityEditor;
using Assets.SNEED.Mazes;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Assets.SNEED.EditorExtensions.CustomInspectors
{
    [CustomEditor(typeof(PathInMaze))]
    public class PathInspector : Editor
    {
        private bool showElements;
        
        PathInMaze instance;

        private LinkedList<MazeUnit> pathInSelection;

        private string pathElementPattern = "{0} {1} = {2} turn {3}";

        public override void OnInspectorGUI()
        {
            instance = target as PathInMaze;
            if (instance != null)
            {
                var maze = instance.GetComponent<beMobileMaze>();

                if (maze == null) throw new MissingComponentException(string.Format("The Path Controller should be attached to a {0} instance", typeof(beMobileMaze).Name));
            }

            EditorGUILayout.BeginVertical();
            
            showElements = EditorGUILayout.Foldout(showElements, "Show Elements");

            if (showElements)
                RenderElements();

            if (GUILayout.Button("Reverse Path"))
            {
                instance.InvertPath();
            }

            EditorGUILayout.Separator();

            EditorGUILayout.EndVertical();
        }

        private void RenderElements()
        {
            EditorGUILayout.BeginVertical();

            foreach (var e in instance.PathAsLinkedList.ToList())
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(
                    string.Format(pathElementPattern, e.Unit.GridID.x, e.Unit.GridID.y,
                    Enum.GetName(typeof(UnitType), e.Type),
                    Enum.GetName(typeof(TurnType), e.Turn)),
                    GUILayout.Width(150f));

                EditorGUILayout.ObjectField(e.Unit, typeof(MazeUnit), false);
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndVertical();
        }
    }
}