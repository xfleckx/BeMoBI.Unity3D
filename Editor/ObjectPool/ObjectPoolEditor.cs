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

                var namePopUp = EditorWindow.GetWindow<CategoryNameDialog>(true);

                var mousePosition = GUIUtility.GUIToScreenPoint(Event.current.mousePosition);

                namePopUp.SetPositionUpOn(mousePosition);


                namePopUp.OnCategoryNameChoosen += name =>
                {
                    var newItem = new GameObject(name, typeof(Category));

                    newItem.transform.parent = instance.transform;

                    var newIndex = l.serializedProperty.arraySize++;

                    l.index = newIndex--;

                    var element = l.serializedProperty.GetArrayElementAtIndex(l.index);

                    element.objectReferenceValue = newItem;

                    EditorUtility.SetDirty(target);
                };

                namePopUp.ShowPopup();
            };


            list.onChangedCallback += (l) =>
            {
                serializedObject.ApplyModifiedProperties();
            };

            list.onRemoveCallback += (l) =>
            {
                var element = list.serializedProperty.GetArrayElementAtIndex(list.index);

                DestroyImmediate(instance.transform.GetChild(list.index).gameObject);

                list.serializedProperty.DeleteArrayElementAtIndex(list.index);

                serializedObject.ApplyModifiedProperties();
            };

            list.drawElementCallback =
                (Rect rect, int index, bool isActive, bool isFocused) =>
                {
                    if (index >= list.count)
                        return;

                    var element = list.serializedProperty.GetArrayElementAtIndex(index);

                    if (element.objectReferenceValue == null)
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

            SceneView.onSceneGUIDelegate -= renderSceneViewElements;
            SceneView.onSceneGUIDelegate += renderSceneViewElements;
        }

        private void renderSceneViewElements(SceneView sceneView)
        {
            if (instance == null) { 
                SceneView.onSceneGUIDelegate -= renderSceneViewElements;
                return;
            }
            Handles.BeginGUI();

            var content = new GUIContent("ObjectPool");

            var size = HandleUtility.GetHandleSize(instance.transform.position) * 0.3f;

            Handles.Label(instance.transform.position + new Vector3(size, size), content);
            
            var screenPos = instance.transform.position;

            var clicked = Handles.Button(screenPos, Quaternion.identity, size, size, Handles.SphereCap);

            if (clicked)
            {
                Selection.activeObject = instance.gameObject;
            }


            Handles.EndGUI();
        }

        void OnDisable()
        {
            SceneView.onSceneGUIDelegate -= renderSceneViewElements;
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
                    serializedObject.ApplyModifiedProperties();
                }
            }
        }

        private Rect inspectorRect;
        public override void OnInspectorGUI()
        {
            instance = target as ObjectPool;

            lookUpCategoriesOn(instance);

            serializedObject.Update();

            list.DoLayoutList();

            EditorGUILayout.BeginVertical();

            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Open Pool Editor", GUILayout.Height(30)))
            {
                ObjectPoolTools.OpenPoolEditor();
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();


            serializedObject.ApplyModifiedProperties();

        }
    }


    public class CategoryNameDialog : EditorWindow
    {
        public Action<string> OnCategoryNameChoosen;

        private string CategoryName;

        public float width = 150;
        public float height = 100;

        internal void SetPositionUpOn(Vector2 mousePosition)
        {
            this.position = new Rect(mousePosition.x - width, mousePosition.y - height, width, height);
        }

        void OnGUI()
        {
            this.titleContent = new GUIContent("Category Name");

            EditorGUILayout.LabelField("Give a name for the new category!", EditorStyles.wordWrappedLabel);

            CategoryName = EditorGUILayout.TextField(CategoryName);

            EditorGUILayout.BeginVertical();

            if (GUILayout.Button("Ok"))
            {
                if (OnCategoryNameChoosen != null)
                    OnCategoryNameChoosen(CategoryName);

                this.Close();
            }

            if (GUILayout.Button("Cancel"))
            {
                this.Close();
            }

            EditorGUILayout.EndVertical();
        }
    }
}