using Assets.SNEED.Mazes;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Assets.SNEED.EditorExtensions.Maze
{
    [CustomEditor(typeof(PathController))]
    public class PathControllerInspector : Editor
    {
        private PathController instance;

        void OnEnable()
        {

            instance = target as PathController;

            instance.ForcePathLookup();
        }

        public override void OnInspectorGUI()
        {
            instance = target as PathController;

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Add new Path"))
            {
                var newPath = instance.gameObject.AddComponent<PathInMaze>();

                newPath.ID = GetNextID();
            }

            if (GUILayout.Button("Re-Numerate Path IDs"))
            {
                var paths = instance.gameObject.GetComponents<PathInMaze>();
                var count = paths.Count();

                for (int i = 0; i < count; i++)
                {
                    paths.ElementAt(i).ID = i;
                }
            }

            EditorGUILayout.EndHorizontal();
        }

        private int GetNextID()
        {
            var paths = instance.gameObject.GetComponents<PathInMaze>();

            var ids = paths.Select((p) => p.ID);

            var highestID = ids.Max();

            return highestID + 1;
        }
    }

}
