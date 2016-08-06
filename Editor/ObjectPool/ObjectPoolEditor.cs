using UnityEngine;
using System.Collections;
using UnityEditor;
using UnityEditorInternal;
using System.Collections.Generic;
using System;
using System.Linq;
using Assets.SNEED.Scripts.ObjectsAndCategories;

namespace Assets.SNEED.EditorExtensions.ObjectsAndCategories
{
    [CustomEditor(typeof(ObjectPool))]
    public class ObjectPoolEditor : Editor
    {
        SerializedProperty serializedCategories;

        ObjectPool instance;
        private ReorderableList list;

        void OnEnable()
        {
            list = new ReorderableList(serializedObject,
                 serializedObject.FindProperty("Categories"),
                 true, true, true, true);

            list.drawHeaderCallback = (Rect rect) =>
            {
                EditorGUI.LabelField(rect, "Categories");
            };

            list.onAddCallback = (l) =>
            {
                var newIndex = l.serializedProperty.arraySize++;
                l.index = newIndex--;
                var element = l.serializedProperty.GetArrayElementAtIndex(l.index);

                var newItem = new GameObject().AddComponent<Category>();

                //var namePopUp = EditorWindow.CreateInstance<ShowPopupExample>();
                //namePopUp.position = new Rect(Screen.width / 2, Screen.height / 2, 250, 150);
                //namePopUp.ShowPopup();

                newItem.transform.parent = instance.transform;
                element.objectReferenceValue = newItem;
            };


            list.onChangedCallback += (l) =>
            {
                serializedObject.ApplyModifiedProperties();
            };

            list.onRemoveCallback += (l) =>
            {
                var element = list.serializedProperty.GetArrayElementAtIndex(list.index);

                var elementToRemoveFromHierachy = instance.transform.AllChildren().Where(
                    c => {
                        var nameOfElement = element.FindPropertyRelative("name");
                        var nameOfElementAsString = nameOfElement.stringValue;
                        return c.name.Equals(nameOfElementAsString);
                        });

                list.serializedProperty.DeleteArrayElementAtIndex(list.index);

                serializedObject.ApplyModifiedProperties();
            };

            list.drawElementCallback =
                (Rect rect, int index, bool isActive, bool isFocused) =>
                {
                    if (index >= list.count)
                        return;

                    var element = list.serializedProperty.GetArrayElementAtIndex(index);
                    
                    if(element.objectReferenceValue == null)
                    {
                        list.serializedProperty.DeleteArrayElementAtIndex(index);
                        serializedObject.ApplyModifiedProperties();
                        return;
                    }

                    rect.y += 2;

                    var displayedProp = element.objectReferenceValue.name;

                    EditorGUI.LabelField(
                        new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight),
                        displayedProp);
                };
        }

        private void lookUpCategoriesOn(ObjectPool instance)
        {
            var allChildren = instance.transform.AllChildren();

            foreach (var child in allChildren)
            {
                var category = child.GetComponent<Category>();

                if (!instance.Categories.Contains(category))
                {
                    instance.Categories.Add(category);
                    EditorUtility.SetDirty(instance);
                }
            }
        }


        public override void OnInspectorGUI()
        {
            instance = target as ObjectPool;

            //lookUpCategoriesOn(instance);
            //if(Event.current.type == EventType.layout)
                list.DoLayoutList();

            EditorGUILayout.BeginVertical();

            EditorGUILayout.Space();

            serializedObject.Update();
            
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

    public class ShowPopupExample : EditorWindow
    {
        static void Init()
        {
            ShowPopupExample window = ScriptableObject.CreateInstance<ShowPopupExample>();
            window.position = new Rect(Screen.width / 2, Screen.height / 2, 250, 150);
            window.ShowPopup();
        }

        void OnGUI()
        {
            EditorGUILayout.LabelField("This is an example of EditorWindow.ShowPopup", EditorStyles.wordWrappedLabel);
            GUILayout.Space(70);
            if (GUILayout.Button("Agree!")) this.Close();
        }
    }
}