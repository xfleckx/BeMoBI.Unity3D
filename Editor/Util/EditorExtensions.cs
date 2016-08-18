using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Assets.SNEED.EditorExtensions.Util
{
    public static class CodeUtils
    {
        public static Vector2 Drag2D(Vector2 scrollPosition, Rect position)
        {
            int controlID = GUIUtility.GetControlID("Slider".GetHashCode(), FocusType.Passive);
            Event current = Event.current;
            switch (current.GetTypeForControl(controlID))
            {
                case EventType.MouseDown:
                    if (position.Contains(current.mousePosition) && position.width > 50f)
                    {
                        GUIUtility.hotControl = controlID;
                        current.Use();
                        EditorGUIUtility.SetWantsMouseJumping(1);
                    }
                    break;
                case EventType.MouseUp:
                    if (GUIUtility.hotControl == controlID)
                    {
                        GUIUtility.hotControl = 0;
                    }
                    EditorGUIUtility.SetWantsMouseJumping(0);
                    break;
                case EventType.MouseDrag:
                    if (GUIUtility.hotControl == controlID)
                    {
                        scrollPosition -= current.delta * (float)((!current.shift) ? 1 : 3) / Mathf.Min(position.width, position.height) * 140f;
                        scrollPosition.y = Mathf.Clamp(scrollPosition.y, -90f, 90f);
                        current.Use();
                        GUI.changed = true;
                    }
                    break;
            }
            return scrollPosition;
        }
        public static void ApplyToAll<T>(this IEnumerable<T> collection, Action<T> function)
        {
            foreach (var item in collection)
            {
                function(item);
            }
        }

        public static bool IsPrefab(this UnityEngine.Object o)
        {

            var prefabType = PrefabUtility.GetPrefabType(o);

            return prefabType == PrefabType.Prefab;
        }

        public static int RenderAsSelectionBox<T>(this IEnumerable<T> list, int selectionIndex)
        {
            int optionCount = list.Count();
            string[] options = new string[optionCount];
            list.Select(i => i.ToString()).ToArray().CopyTo(options, 0);
            selectionIndex = EditorGUILayout.Popup(selectionIndex, options);
            return selectionIndex;
        }


        public static IEnumerable<SceneView> GetAllSceneViews()
        {
            foreach (var view in SceneView.sceneViews)
            {
                yield return view as SceneView;
            }
        }
    }
}