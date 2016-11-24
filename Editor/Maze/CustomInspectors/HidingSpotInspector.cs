using UnityEngine;
using UnityEditor;
using Assets.SNEED.Mazes;

namespace Assets.SNEED.EditorExtensions.CustomInspectors
{
    [CustomEditor(typeof(HidingSpot))]
    public class HidingSpotInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            var instance = target as HidingSpot;

            base.OnInspectorGUI();

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Reveal"))
            {
                instance.Reveal();
            }

            if (GUILayout.Button("Hide"))
            {
                instance.Hide();
            }

            EditorGUILayout.EndHorizontal();
        }
    }
}