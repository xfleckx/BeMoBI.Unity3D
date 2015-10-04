using UnityEngine;
using UnityEditor;
using System;

public class MazeCustomizer : EditorWindow
{
    [MenuItem("BeMoBI/Maze/Customizer")]
    static void Init()
    {
        var window = EditorWindow.GetWindow<MazeCustomizer>();

        window.titleContent = new GUIContent( "Maze Customizer" );

        window.Show();
    }



    void OnGUI()
    {
        if(Selection.activeGameObject == null) {
            renderUiForNoMazeSelected();
            return;
        };

        if(Selection.gameObjects.Length > 1)
        {
            renderUiForMultipleMazesSelected();
            return;
        }

        selectedMaze = Selection.activeGameObject.GetComponent<beMobileMaze>();

        if (selectedMaze != null)
        {
            EditorGUILayout.BeginVertical();

            renderUiForSingleMazeSelected();

            EditorGUILayout.Space();

            renderUiForUnitSetCustomization();

            EditorGUILayout.EndVertical();
        }
        
    }

    GameObject lightPrefab;

    private void renderUiForUnitSetCustomization()
    {
        EditorGUILayout.BeginVertical();

        EditorGUILayout.LabelField("Lighting", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();

        EditorGUILayout.BeginVertical();

        RenderUIForRequestedPrefab<GameObject, TopLighting>(ref lightPrefab, typeof(TopLighting));

        if (GUILayout.Button("Add Lighting to each unit.", GUILayout.Height(50f)))
        {
            AddLightingToMaze();
        }


        if (GUILayout.Button("Remove Lighting to each unit.", GUILayout.Height(20f)))
        {
            RemoveLightingFromMaze();
        }
        EditorGUILayout.EndHorizontal();

        if (lightPrefab)
        {
            var previewTexture = AssetPreview.GetAssetPreview(lightPrefab);

            GUILayout.Box(previewTexture);
        }

        EditorGUILayout.EndHorizontal();


        EditorGUILayout.EndVertical();
    }
     
    GameObject replacementPrefab;

    private beMobileMaze selectedMaze;

    private void renderUiForSingleMazeSelected()
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.BeginVertical();

        replacementPrefab = (GameObject) EditorGUILayout.ObjectField(replacementPrefab, typeof(UnityEngine.GameObject), false);

        MazeUnit mazeUnit = null;

        string errorMessage = "No prefab selected!";

        if (replacementPrefab != null) { 
           mazeUnit = replacementPrefab.GetComponent<MazeUnit>();

        }
        else
        {
            EditorGUILayout.HelpBox(errorMessage, MessageType.Error);
        }


        if (mazeUnit == null) {

            if (replacementPrefab != null) { 
                errorMessage = "Selected Asset is no MazeUnit!";

                EditorGUILayout.HelpBox(errorMessage, MessageType.Error);
            }
        }
        else
        {
            if (GUILayout.Button(new GUIContent("Replace Maze Units \n with this prefab.", ToolTip_Replace_Units), GUILayout.Height(50f)))
            {
                CallReplaceAlgorithm();
            }

            EditorGUILayout.HelpBox(MsgBox_Replace_Action, MessageType.Warning);
        }

        EditorGUILayout.EndVertical();

        if (replacementPrefab) { 

            var previewTexture = AssetPreview.GetAssetPreview(replacementPrefab);

            GUILayout.Box(previewTexture);
        }

        EditorGUILayout.EndHorizontal();
    }

    private void RenderUIForRequestedPrefab<T, TC>(ref T targetPrefab, Type expectedComponent ) where T : UnityEngine.Object where TC : UnityEngine.MonoBehaviour
    { 
        EditorGUILayout.BeginVertical();

        targetPrefab = (T) EditorGUILayout.ObjectField(targetPrefab, typeof(T), false);

        TC targetComponent = null;

        string errorMessage = "No prefab selected!";

        if (targetPrefab != null && targetPrefab is GameObject)
        {
            targetComponent = (targetPrefab as GameObject).GetComponent<TC>();
        }
        else
        {
            EditorGUILayout.HelpBox(errorMessage, MessageType.Error);
        }

        if (targetComponent == null)
        {
            if (targetPrefab != null)
            {
                errorMessage = string.Format( "Selected Asset is no {0}", typeof(TC).Name);

                EditorGUILayout.HelpBox(errorMessage, MessageType.Error);
            }
        }

        EditorGUILayout.EndVertical();
    }

    private void CallReplaceAlgorithm()
    {
        Debug.Assert(replacementPrefab != null && selectedMaze != null);

        MazeEditorUtil.ReplaceUnits(selectedMaze, replacementPrefab);
        
    }

    private void AddLightingToMaze()
    {
        foreach (var unit in selectedMaze.Units)
        {
            var top = unit.gameObject.transform.FindChild("Top"); 

            var newLightInstance = PrefabUtility.InstantiatePrefab(lightPrefab) as GameObject;

            newLightInstance.transform.parent = top.transform;
            newLightInstance.transform.localPosition = new Vector3(0, -0.001f, 0);
            newLightInstance.transform.localScale = new Vector3(selectedMaze.RoomDimension.x, 1, selectedMaze.RoomDimension.z);
        }
    }

    private void RemoveLightingFromMaze()
    {
        foreach (var unit in selectedMaze.Units)
        {
            var top = unit.gameObject.transform.FindChild("Top");

            var lightning = top.FindChild("TopLighting");

            DestroyImmediate(lightning.gameObject);
        }
    }

    #region TODO
    private void renderUiForMultipleMazesSelected()
    {
         
    }

    private void renderUiForNoMazeSelected()
    {
        

    }
    #endregion


    #region constants

    const string ToolTip_Replace_Units = "The the configuration of each Unit will be obtained";
    const string MsgBox_Replace_Action = "This action will replace all existing Units of the selected maze but obtains the structure of the maze and the configuration of each unit.";
    #endregion
}