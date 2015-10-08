using UnityEngine;
using UnityEditor;
using System.Linq;
using System;

public class ObjectPoolTools : EditorWindow
{ 
    [MenuItem("BeMoBI/Object Pool/Open Pool Editor")]
    static void OpenPoolEditor()
    {
        var w = EditorWindow.CreateInstance<ObjectPoolTools>();

        w.titleContent = new GUIContent( "Object Pool Tools" );

        w.Initialize();

        w.Show();
    }

    private ObjectPool currentPool;  
    private void Initialize()
    {
        CheckIfAObjectPoolOrCategoryIsSelected();
    }

    private Type CheckIfAObjectPoolOrCategoryIsSelected()
    {
        var go = Selection.activeGameObject;

        if (go == null)
            return null;

        currentPool = go.GetComponent<ObjectPool>();

        if (currentPool != null)
            return typeof(ObjectPool);

        selectedCategory = go.GetComponent<Category>();

        if (selectedCategory != null)
            return typeof(Category);

        return null;
    }

    void OnGUI()
    {
        var selected = CheckIfAObjectPoolOrCategoryIsSelected();

        if(selected == null){ 
            renderUiForPoolSelectionOrCreation();
            return; 
           
        }

        if (selected == typeof(ObjectPool)) { 
            renderUiForPoolEditing();
            return;

        }
        else if (selected == typeof(Category))
        {  
            renderUiForCategoryEditing();
            return;
        }
    
    }


    #region Object pool

    private string newPoolName = "ObjectPool";

    private void renderUiForPoolSelectionOrCreation()
    {
        newPoolName = GUILayout.TextField(newPoolName);

        if (GUILayout.Button("Create new Pool")) { 
            CreateNewPoolInScene(newPoolName);
            Repaint();
        }

        var availablePools = GameObject.FindObjectsOfType<ObjectPool>();

        if (availablePools != null && availablePools.Any()) { 

        GUILayout.Label("Select one from available:");

            foreach (var item in availablePools)
            {
                if (GUILayout.Button(item.name)) {
                    Selection.activeObject = item;
                    Repaint();
                }
            }
        }
        //if (GUILayout.Button("Create Instance \n from prefab"))

    }

    private void renderUiForPoolEditing()
    {
        EditorGUILayout.BeginVertical();

        newCategoryName = GUILayout.TextField(newCategoryName);

        if (!currentPool.Categories.Any(c => c.name == null || c.name == String.Empty || c.name.Equals(newCategoryName)))
        {
            if (GUILayout.Button("Add Category"))
            {
                CreateNewCategory(currentPool, newCategoryName);
            }
        }
        else
        {
            EditorGUILayout.HelpBox("Category name empty or already existing!", MessageType.Error);
        }

        if (GUILayout.Button("Save as object pool as prefab"))
        {
           var filePath = EditorUtility.SaveFilePanelInProject("Save object pool as prefab", "objectPool", "prefab", "");

           var prefab = PrefabUtility.CreatePrefab(filePath, currentPool.gameObject);
           
           AssetDatabase.SaveAssets();
        }

        EditorGUILayout.EndVertical();
    }

    private void CreateNewPoolInScene(string name)
    {
        var host = new GameObject(name);

        var pool = host.AddComponent<ObjectPool>();

        currentPool = pool; 
    }
    
    #endregion

    #region Category

    private void renderUiForCategorySelectionOrCreation()
    {

    }

    private void renderUiForCategoryEditing()
    {
        if(GUILayout.Button("Add Object from prefab")){

            string pathToPrefab = EditorUtility.OpenFilePanel("Prefab selection", Application.dataPath, "prefab");

            pathToPrefab = pathToPrefab.Replace(Application.dataPath,"Assets");

            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(pathToPrefab);

            Debug.Assert(prefab != null, "Loading the prefab failed!");

            GameObject prefabInstance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;

            prefabInstance.transform.parent = selectedCategory.transform;
            prefabInstance.SetActive(false);
            selectedCategory.AssociatedObjects.Add(prefabInstance);
        }
    }

    private void CheckCategoryListConsistency(ObjectPool targetPool)
    {
        var isNull = new Func<Category,bool>(c => c == null);
         
        if(currentPool.Categories.Any(isNull)){
            currentPool.Categories.RemoveAll(c => c == null);
        }
    }

    private void CreateNewCategory(ObjectPool targetPool, string categoryName)
    {
        var host = new GameObject(categoryName);

        host.transform.parent = targetPool.transform;

        var newCategory = host.AddComponent<Category>();

        targetPool.Categories.Add(newCategory);

        CheckCategoryListConsistency(targetPool);
    }

    private string newCategoryName = "newCategory";
    private Category selectedCategory;

    #endregion
}