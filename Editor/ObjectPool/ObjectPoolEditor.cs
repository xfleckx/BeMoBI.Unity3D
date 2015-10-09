using UnityEngine;
using System.Collections;
using UnityEditor;
using UnityEditorInternal;
using System.Collections.Generic;

[CustomEditor(typeof(ObjectPool))]
public class ObjectPoolEditor : Editor {

    ObjectPool instance;
    private ReorderableList list;

    void OnEnable()
    {
        list = new ReorderableList(serializedObject,
             serializedObject.FindProperty("Categories"),
             true, true, false, false);

        list.drawHeaderCallback = (Rect rect) => {  
                EditorGUI.LabelField(rect, "Categories");
            };

        list.onAddCallback = (l) =>
        {
            var index = l.serializedProperty.arraySize;
            l.serializedProperty.arraySize++;
            l.index = index;
            var element = l.serializedProperty.GetArrayElementAtIndex(index);
            var newItem = new GameObject().AddComponent<Category>();
            newItem.transform.parent = instance.transform;
            element.objectReferenceValue = newItem;
        };

        list.drawElementCallback =  
            (Rect rect, int index, bool isActive, bool isFocused) => {

                var element = list.serializedProperty.GetArrayElementAtIndex(index);
                rect.y += 2;

                var displayedProp = element.objectReferenceValue.name;

                EditorGUI.LabelField(
                    new Rect(rect.x, rect.y, 60, EditorGUIUtility.singleLineHeight),
                    displayedProp);
        };
    }

    public override void OnInspectorGUI()
    {
        instance = target as ObjectPool;

        EditorGUILayout.BeginVertical();

        EditorGUILayout.Space();

        serializedObject.Update();

        list.DoLayoutList();

        serializedObject.ApplyModifiedProperties();

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Open Pool Editor", GUILayout.Height(30)))
        {
            ObjectPoolTools.OpenPoolEditor();
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();
    }

}
