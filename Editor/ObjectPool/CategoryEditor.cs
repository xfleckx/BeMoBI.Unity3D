using UnityEngine;
using System.Collections;
using UnityEditor;
using UnityEditorInternal;

[CustomEditor(typeof(Category))]
public class CategoryEditor : Editor {

    Category instance;

    private int currentPreviewIndex = 0;
    private int lastPreviewIndex = 0;

    private GameObject currentPreviewObject;

    public override void OnInspectorGUI()
    {
        instance = target as Category;

        EditorGUILayout.BeginVertical();

        var objectCount = instance.AssociatedObjects.Count;

        if (objectCount == 0)
        {
            EditorGUILayout.HelpBox("No Objects in this category... Add them with the Object Pool Tools.", MessageType.Info);

            return;
        }

        GUILayout.Label(string.Format("Switch through {0} available objects", objectCount), EditorStyles.largeLabel);
        
        EditorGUILayout.Space();

        if(currentPreviewObject != null)
         GUILayout.Label(string.Format("Show: {0}, {1}", currentPreviewIndex, currentPreviewObject.name), EditorStyles.whiteBoldLabel);

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Previous"))
        {
            lastPreviewIndex = currentPreviewIndex;

            currentPreviewIndex = currentPreviewIndex - 1 < 0 ? objectCount -1 : currentPreviewIndex - 1;

            SetPreviewObject(instance.AssociatedObjects[currentPreviewIndex]);
        }

        if (GUILayout.Button("Next"))
        {
            lastPreviewIndex = currentPreviewIndex;

            currentPreviewIndex = currentPreviewIndex + 1 > objectCount - 1 ? 0 : currentPreviewIndex + 1 ;

            SetPreviewObject(instance.AssociatedObjects[currentPreviewIndex]);
        }
        
        EditorGUILayout.EndHorizontal();
       
        EditorGUILayout.Space();
        
        GUILayout.Label("Random sample a object");

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("With replacement"))
        {
            SetPreviewObject(instance.Sample());
        }

        if (GUILayout.Button("Without replacement"))
        {
            SetPreviewObject(instance.SampleWithoutReplacement());
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndVertical(); 
    }

    void OnDisable()
    {
        if (currentPreviewObject != null)
            currentPreviewObject.SetActive(false);
    }

    private void SetPreviewObject(GameObject newPreview)
    {
        if (currentPreviewObject != null)
        {
            currentPreviewObject.SetActive(false);
        }  

        currentPreviewObject = newPreview;

        currentPreviewObject.SetActive(true);
    }
     
}
