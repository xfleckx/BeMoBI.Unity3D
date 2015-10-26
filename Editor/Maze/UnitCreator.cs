using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System;
using System.IO;
using TUBerlin.Bpn.BeMoBI.EditorGUIExtensions;
using Assets.BeMoBI.Unity3D.Editor.Maze.UnitCreation;

public class UnitCreator : EditorWindow
{ 
    [MenuItem("BeMoBI/Maze/Unit Creator")]
    static void OpenUnitCreator()
    {
        var window = EditorWindow.GetWindow<UnitCreator>();

        window.titleContent = new GUIContent("Unit Creator");
        
        window.Initialize();

        window.Show();
    }

    void Initialize()
    {

        if (hidingSpotCreator == null) { 
            hidingSpotCreator = CreateAndInitialize<HidingSpotCreator>();
        }

        if (baseUnitCreator == null){
            baseUnitCreator = CreateAndInitialize<BasicUnitCreator>();
        }
    }

    private T CreateAndInitialize<T>() where T : CreatorState
    {
        var t = CreateInstance<T>();
        t.hideFlags = HideFlags.HideAndDontSave;

        t.Initialize();

        return t;
    }

    private bool basicUnitSelected = true;

    private bool hidingSpotSelected = false;

    private HidingSpotCreator hidingSpotCreator;
    private BasicUnitCreator baseUnitCreator;

    #region Toogle Button
    public bool ToggleButton(bool state, string label)
    {
        BuildStyle();

        bool out_bool = false;

        if (state)
            out_bool = GUILayout.Button(label, toggled_style);
        else
            out_bool = GUILayout.Button(label);

        if (out_bool)
            return !state;
        else
            return state;
    }

    static GUIStyle toggled_style;
    public static GUIStyle StyleButtonToggled
    {
        get
        {
            return toggled_style;
        }
    }

    static GUIStyle labelText_style;
    private string doorPrefabName;
    public static GUIStyle StyleLabelText
    {
        get
        {
            return labelText_style;
        }
    }

    private void BuildStyle()
    {
        if (toggled_style == null)
        {
            toggled_style = new GUIStyle(GUI.skin.button);
            toggled_style.normal.background = toggled_style.onActive.background;
            toggled_style.normal.textColor = toggled_style.onActive.textColor;
        }
        if (labelText_style == null)
        {
            labelText_style = new GUIStyle(EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector).textField);
            labelText_style.normal = EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector).button.onNormal;
        }
    }

    #endregion

    void OnGUI()
    {
        EditorGUILayout.BeginHorizontal();

        basicUnitSelected =  ToggleButton(!hidingSpotSelected, baseUnitCreator.CreatorName);
        hidingSpotSelected =  ToggleButton(!basicUnitSelected, hidingSpotCreator.CreatorName);
        
        EditorGUILayout.EndHorizontal();

        #region basic Unit

        if (basicUnitSelected) {
            baseUnitCreator.OnGUI();
        }
        #endregion

        #region HidingSpot Creation
        
        if (hidingSpotSelected)
        {
            hidingSpotCreator.OnGUI();
        }

        #endregion
    }
      
    public void OnEnable()
    {
        if (SceneView.onSceneGUIDelegate == null)
            SceneView.onSceneGUIDelegate += OnSceneGUI;
    }

    public void OnDisable()
    {
        SceneView.onSceneGUIDelegate -= OnSceneGUI;
    }

    private void OnSceneGUI(SceneView sceneView)
    {
        if (Selection.activeObject != null && Selection.activeObject is GameObject)
        {
            var go = Selection.activeObject as GameObject;

            var expectedMeshFilter = go.GetComponent<MeshFilter>();

            if (expectedMeshFilter == null)
                return;

            if (expectedMeshFilter.sharedMesh == null)
                return;
            
            var temp = Handles.matrix;

            Handles.matrix = go.transform.localToWorldMatrix;

            var vertices = expectedMeshFilter.sharedMesh.vertices;
            var vertexCount = vertices.Length;

            for (int i = 0; i < vertexCount; i++)
            {
                //var index = indices[i];
                var vertex = vertices[i];

                Handles.CubeCap(0, vertex, Quaternion.identity, 0.01f);
                
                var info = vertex.ToString();

                Handles.Label(vertex, info);
            }

            Handles.matrix = temp;

        }
    } 

}

