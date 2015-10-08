using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System;
using System.IO;

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


    List<String> expectedChildComponents;

    private Vector3 roomDimensions = Vector3.one;
    private Vector3 roomOrigin = Vector3.zero;


    private bool useCenterAsOrigin = true;
    private bool addBoxCollider = true;
    private bool createAsWholeMesh = false;

    private string prefabName = "MazeUnit";

    private GameObject constructedUnit;
    private GameObject prefabReference;

    void Initialize()
    {
        expectedChildComponents = new List<string>() { MazeUnit.TOP, MazeUnit.FLOOR, MazeUnit.NORTH, MazeUnit.SOUTH, MazeUnit.WEST, MazeUnit.EAST };


    }

    void OnGUI()
    {
        EditorGUILayout.BeginVertical();
        
        roomDimensions = EditorGUILayout.Vector3Field("Room Dimension", roomDimensions);
        roomOrigin = EditorGUILayout.Vector3Field("Origin",  roomOrigin);

        useCenterAsOrigin = EditorGUILayout.ToggleLeft("Use the Center as origin!", useCenterAsOrigin);

        addBoxCollider = EditorGUILayout.ToggleLeft("Add Box Collider", addBoxCollider);

        createAsWholeMesh = EditorGUILayout.ToggleLeft("Create as whole Mesh", createAsWholeMesh);

        if (GUILayout.Button("Create new Maze Unit (Room)", GUILayout.Height(35f)))
        {
            if (expectedChildComponents != null)
                Initialize();

            var resizeModel = new UnitMeshModificationModel(roomDimensions, roomOrigin, useCenterAsOrigin);
           constructedUnit = ConstructUnit(resizeModel);
        }

        if (GUILayout.Button("Resize Unit (Room)"))
        {
            var resizeModel = new UnitMeshModificationModel(roomDimensions, roomOrigin, useCenterAsOrigin);

            MazeEditorUtil.ResizeUnitByMeshModification(constructedUnit, resizeModel);
        }

        constructedUnit = EditorGUILayout.ObjectField("Created Unit", constructedUnit, typeof(GameObject), true) as GameObject;

        EditorGUILayout.Space();

        prefabName = EditorGUILayout.TextField("Prefab Name:", prefabName);

        if (constructedUnit != null && prefabReference == null && GUILayout.Button("Save as Prefab"))
        {
            var targetPath = EditorEnvironmentConstants.Get_PREFAB_DIR_PATH();

            Debug.Assert(AssetDatabase.IsValidFolder(targetPath), string.Format("Expected prefab folder at \"{0}\"", targetPath));

            var targetFilePath = targetPath + Path.AltDirectorySeparatorChar + string.Format("{0}{1}", prefabName, EditorEnvironmentConstants.PREFAB_EXTENSION);

            PrefabUtility.CreatePrefab(targetFilePath, constructedUnit);
        }

        EditorGUILayout.EndVertical();
    }
      
    private GameObject ConstructUnit(UnitMeshModificationModel creationModel)
    {
        var newUnit = new GameObject("MazeUnit");

        newUnit.AddComponent<MazeUnit>();

        var boxCollider = newUnit.AddComponent<BoxCollider>();

        boxCollider.center = new Vector3(0, roomDimensions.y / 2, 0);
        
        boxCollider.size = roomDimensions;

        var prototype = GameObject.CreatePrimitive(PrimitiveType.Plane);
        prototype.hideFlags = HideFlags.HideAndDontSave;
        var prototypeMeshRenderer = prototype.GetComponent<MeshRenderer>();
        var material = prototypeMeshRenderer.sharedMaterial;

        if (!createAsWholeMesh)
            CreateAsSeparatedMeshes(newUnit, creationModel, material);
        
        if (createAsWholeMesh)
            CreateAsAWhole(newUnit, creationModel, material);

        DestroyImmediate(prototype);

        return newUnit; 
    }

    #region create Mesh in separate pieces

    private void CreateAsSeparatedMeshes(GameObject newUnit, UnitMeshModificationModel creationModel, Material material)
    {
        foreach (var item in expectedChildComponents)
        {
            var wall = new GameObject(item);

            wall.transform.parent = newUnit.transform;

            var meshFilter = wall.AddComponent<MeshFilter>();

            var meshRenderer = wall.AddComponent<MeshRenderer>();

            meshRenderer.material = material;

            if (item.Equals(MazeUnit.TOP))
            {
                var topMesh = CreateTopMesh(creationModel);

                meshFilter.mesh = topMesh;
                meshFilter.sharedMesh.name = MazeUnit.TOP;
                wall.transform.localPosition = V(0, roomDimensions.y, 0);
            }

            if (item.Equals(MazeUnit.FLOOR))
            {
                meshFilter.mesh = CreateFloorMesh(creationModel);
                meshFilter.sharedMesh.name = MazeUnit.FLOOR;
            }

            if (item.Equals(MazeUnit.NORTH))
            {
                wall.transform.localPosition = V(0, roomDimensions.y / 2, roomDimensions.z / 2);
                meshFilter.mesh = CreateNorthMesh(creationModel);
                meshFilter.sharedMesh.name = MazeUnit.NORTH;

            }

            if (item.Equals(MazeUnit.SOUTH))
            {
                meshFilter.mesh = CreateSouthMesh(creationModel);
                meshFilter.sharedMesh.name = MazeUnit.SOUTH; wall.transform.localPosition = V(0, roomDimensions.y / 2, -roomDimensions.z / 2);
            }

            if (item.Equals(MazeUnit.WEST))
            {
                meshFilter.mesh = CreateWestMesh(creationModel);
                meshFilter.sharedMesh.name = MazeUnit.WEST;
                wall.transform.localPosition = V(-roomDimensions.x / 2, roomDimensions.y / 2, 0);
            }

            if (item.Equals(MazeUnit.EAST))
            {
                meshFilter.mesh = CreateEastMesh(creationModel);
                meshFilter.sharedMesh.name = MazeUnit.EAST;
                wall.transform.localPosition = V(roomDimensions.x / 2, roomDimensions.y / 2, 0);
            }
        }
    }

    private Mesh CreateFloorMesh(UnitMeshModificationModel model)
    {
        var m = model;

        var mesh = new Mesh();

        var vertices = new List<Vector3>()
        {
            V(m.meshOrigin.x,      0, m.meshOrigin.z), 
            V(m.WidthEndPoint, 0, m.meshOrigin.z),
            V(m.meshOrigin.x,      0, m.DepthEndPoint),
            V(m.WidthEndPoint, 0, m.DepthEndPoint)
        }; 

        mesh.SetVertices(vertices);

        var triangles = new List<int>()
        {
            0,2,1,
            2,3,1
        };

        mesh.SetTriangles(triangles, 0);

        var normals = new List<Vector3>()
        {
            Vector3.up,
            Vector3.up,
            Vector3.up,
            Vector3.up
        };

        mesh.SetNormals(normals);

        var uvs = new List<Vector2>()
        {
            Vector2.zero,
            V(0, 1),
            Vector2.one,
            V(1, 0)
        };


        mesh.SetUVs(0, uvs);

        mesh.RecalculateBounds();
        mesh.Optimize();

        return mesh;
    }

    private Mesh CreateTopMesh(UnitMeshModificationModel model)
    {
        var m = model;

        var mesh = new Mesh();

        var vertices = new List<Vector3>()
        {
            V(m.meshOrigin.x,         0, m.meshOrigin.z),
            V(m.WidthEndPoint,    0, m.meshOrigin.z),
            V(m.meshOrigin.x,         0, m.DepthEndPoint),
            V(m.WidthEndPoint,    0, m.DepthEndPoint)
        }; 

        mesh.SetVertices(vertices);

        var triangles = new List<int>()
        {
            1,2,0,
            1,3,2
        };

        mesh.SetTriangles(triangles, 0);

        var normals = new List<Vector3>()
        {
            Vector3.down,
            Vector3.down,
            Vector3.down,
            Vector3.down
        };

        mesh.SetNormals(normals);

        var uvs = new List<Vector2>()
        {
            Vector2.zero,
            V(0, 1),
            Vector2.one,
            V(1, 0)
        };

        mesh.SetUVs(0, uvs);

        mesh.RecalculateBounds();
        mesh.Optimize();

        return mesh;
    }

    private Mesh CreateNorthMesh(UnitMeshModificationModel model)
    {
        var m = model;

        var mesh = new Mesh();

        var vertices = new List<Vector3>()
        {
            V(m.meshOrigin.x,         m.meshOrigin.y,           0),
            V(m.WidthEndPoint,    m.meshOrigin.y,           0),
            V(m.meshOrigin.x,         m.HeightEndPoint,      0),
            V(m.WidthEndPoint,    m.HeightEndPoint,     0)
        };

        mesh.SetVertices(vertices);

        var triangles = new List<int>()
        {
            0,2,1,
            2,3,1
        };

        mesh.SetTriangles(triangles, 0);

        var normals = new List<Vector3>()
        {
            Vector3.back,
            Vector3.back,
            Vector3.back,
            Vector3.back 
            
        };

        mesh.SetNormals(normals);

        var uvs = new List<Vector2>()
        {
            Vector2.zero,
            V(0, 1),
            Vector2.one,
            V(1, 0)
        };

        mesh.SetUVs(0, uvs);

        mesh.RecalculateBounds();
        mesh.Optimize();

        return mesh;
    }

    private Mesh CreateSouthMesh(UnitMeshModificationModel model)
    {
        var m = model;

        var mesh = new Mesh();

        var vertices = new List<Vector3>()
        {
            V(m.meshOrigin.x,         m.meshOrigin.y,        0),
            V(m.WidthEndPoint,    m.meshOrigin.y,        0),
            V(m.meshOrigin.x,         m.HeightEndPoint,  0),
            V(m.WidthEndPoint,    m.HeightEndPoint,  0)
        };

        mesh.SetVertices(vertices);

        var triangles = new List<int>()
        {
            1,2,0,
            1,3,2
        };

        mesh.SetTriangles(triangles, 0);

        var normals = new List<Vector3>()
        {Vector3.forward, 
            Vector3.forward, 
            Vector3.forward, 
            Vector3.forward 
        };

        mesh.SetNormals(normals);

        var uvs = new List<Vector2>()
        {
            Vector2.zero,
            V(0, 1),
            Vector2.one,
            V(1, 0)
        };

        mesh.SetUVs(0, uvs);

        mesh.RecalculateBounds();
        mesh.Optimize();

        return mesh;
    }

    private Mesh CreateWestMesh(UnitMeshModificationModel model)
    {
        var m = model;

        var mesh = new Mesh();

        var vertices = new List<Vector3>()
        {
            V(0,   m.meshOrigin.y,         m.meshOrigin.z),
            V(0,   m.HeightEndPoint,   m.meshOrigin.z),
            V(0,   m.meshOrigin.y,         m.DepthEndPoint),
            V(0,   m.HeightEndPoint,   m.DepthEndPoint)
        };

        mesh.SetVertices(vertices);

        var triangles = new List<int>()
        {
            1,2,0,
            1,3,2
        };

        mesh.SetTriangles(triangles, 0);

        var normals = new List<Vector3>()
        {
            
            Vector3.right, 
            Vector3.right, 
            Vector3.right, 
            Vector3.right 
            
        };

        mesh.SetNormals(normals);

        var uvs = new List<Vector2>()
        {
            Vector2.zero,
            V(0, 1),
            Vector2.one,
            V(1, 0)
        };

        mesh.SetUVs(0, uvs);

        mesh.RecalculateBounds();
        mesh.Optimize();

        return mesh;
    }

    private Mesh CreateEastMesh(UnitMeshModificationModel model)
    {
        var m = model;

        var mesh = new Mesh();

        var vertices = new List<Vector3>()
        {
            V(0,   m.meshOrigin.y,         m.meshOrigin.z),
            V(0,   m.HeightEndPoint,   m.meshOrigin.z),
            V(0,   m.meshOrigin.y,         m.DepthEndPoint),
            V(0,   m.HeightEndPoint,   m.DepthEndPoint)
        };

        mesh.SetVertices(vertices);

        var triangles = new List<int>()
        {
            0,2,1,
            2,3,1
        };

        mesh.SetTriangles(triangles, 0);

        var normals = new List<Vector3>()
        { Vector3.left,
            Vector3.left,
            Vector3.left,
            Vector3.left 
        };

        mesh.SetNormals(normals);

        var uvs = new List<Vector2>()
        {
            Vector2.zero,
            V(0, 1),
            Vector2.one,
            V(1, 0)
        };

        mesh.SetUVs(0, uvs);

        mesh.RecalculateBounds();
        mesh.Optimize();


        return mesh;
    }

    #endregion

    #region create Mesh as a whole thing

    private void CreateAsAWhole(GameObject newUnit, UnitMeshModificationModel creationModel, Material material)
    {
        var m = creationModel;

        var mesh = new Mesh();

        float length = m.RoomDimensions.z;
        float width = m.RoomDimensions.x;
        float height = m.RoomDimensions.y;

        #region Vertices

        var p0 = V(-m.WidthStartPoint, -m.HeightStartPoint, -m.DepthStartPoint);
        var p1 = V(length * .5f, -width * .5f, height * .5f);
        var p2 = V(length * .5f, -width * .5f, -height * .5f);
        var p3 = V(-length * .5f, -width * .5f, -height * .5f);

        var p4 = V(-length * .5f, width * .5f, height * .5f);
        var p5 = V(length * .5f, width * .5f, height * .5f);
        var p6 = V(length * .5f, width * .5f, -height * .5f);
        var p7 = V(-length * .5f, width * .5f, -height * .5f);

        var vertices = new Vector3[]
            {
	            // Bottom
	            p0, p1, p2, p3,
 
	            // Left
	            p7, p4, p0, p3,
 
	            // Front
	            p4, p5, p1, p0,
 
	            // Back
	            p6, p7, p3, p2,
 
	            // Right
	            p5, p6, p2, p1,
 
	            // Top
	            p7, p6, p5, p4
            };

        #endregion

        #region Normales
        var up = Vector3.up;
        var down = Vector3.down;
        var front = Vector3.forward;
        var back = Vector3.back;
        var left = Vector3.left;
        var right = Vector3.right;

        Vector3[] normales = new Vector3[]
        {
	        // Bottom
	        down, down, down, down,
 
	        // Left
	        left, left, left, left,
 
	        // Front
	        front, front, front, front,
 
	        // Back
	        back, back, back, back,
 
	        // Right
	        right, right, right, right,
 
	        // Top
	        up, up, up, up
        };

        var flipedNormals = new Vector3[]{
            
	        // Bottom
	        up, up, up, up,

            // Left
	        right, right, right, right,

            // Front
            back, back, back, back,

            // back 
            front, front, front, front,

            // Right
            left, left, left, left,
 
            // Top
            down, down, down, down
        };

        #endregion	

        #region UVs
        var _00 = new Vector2(0f, 0f);
        var _10 = new Vector2(1f, 0f);
        var _01 = new Vector2(0f, 1f);
        var _11 = new Vector2(1f, 1f);

        Vector2[] uvs = new Vector2[]
        {
	        // Bottom
	        _11, _01, _00, _10,
 
	        // Left
	        _11, _01, _00, _10,
 
	        // Front
	        _11, _01, _00, _10,
 
	        // Back
	        _11, _01, _00, _10,
 
	        // Right
	        _11, _01, _00, _10,
 
	        // Top
	        _11, _01, _00, _10,
        };
        #endregion

        #region Triangles
        int[] triangles = new int[]
        {
	        // Bottom
	        0, 1, 3,
	        1, 2, 3,			
 
	        // Left
	        0 + 4 * 1, 1 + 4 * 1, 3 + 4 * 1,
	        1 + 4 * 1, 2 + 4 * 1, 3 + 4 * 1,
 
	        // Front
	        0 + 4 * 2, 1 + 4 * 2, 3 + 4 * 2,
	        1 + 4 * 2, 2 + 4 * 2, 3 + 4 * 2,
 
	        // Back
	        0 + 4 * 3, 1 + 4 * 3, 3 + 4 * 3,
	        1 + 4 * 3, 2 + 4 * 3, 3 + 4 * 3,
 
	        // Right
	        0 + 4 * 4, 1 + 4 * 4, 3 + 4 * 4,
	        1 + 4 * 4, 2 + 4 * 4, 3 + 4 * 4,
 
	        // Top
	        0 + 4 * 5, 1 + 4 * 5, 3 + 4 * 5,
	        1 + 4 * 5, 2 + 4 * 5, 3 + 4 * 5,
 
        };
        #endregion

        mesh.vertices = vertices;
        //mesh.normals = normales;
        mesh.normals = flipedNormals;
        mesh.uv = uvs;
        mesh.triangles = triangles;

        mesh.RecalculateBounds();
        mesh.Optimize();

        var meshFilter = newUnit.AddComponent<MeshFilter>();
        var meshRenderer = newUnit.AddComponent<MeshRenderer>();

        meshFilter.mesh = mesh;

        meshRenderer.material = material;

    }

    #endregion

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
            //var indices = expectedMeshFilter.sharedMesh.GetIndices(0);
            
            //var indexCount = indices.Length;

            for (int i = 0; i < vertexCount; i++)
            {
                //var index = indices[i];
                var vertex = vertices[i];

                Handles.CubeCap(0, vertex, Quaternion.identity, 0.01f);
                
                //var info = string.Format("{0}:{1}", index, vertex.ToString());

                var info = vertex.ToString();


                Handles.Label(vertex, info);
            }

            Handles.matrix = temp;

        }
    } 

    #region Helper

    private Vector3 V(float x, float y, float z)
    {
        return new Vector3(x, y, z);
    }
    private Vector2 V(float x, float y)
    {
        return new Vector2(x, y);
    }

    #endregion
}


public class UnitMeshModificationModel
{
    public UnitMeshModificationModel(Vector3 dimensions, Vector3 origin, bool centerOrigin)
    {
        roomDimensions = dimensions;
        roomOrigin = origin;
        useCenterAsOrigin = centerOrigin;
    }

    private Vector3 roomDimensions = Vector3.one;

    public Vector3 RoomDimensions
    {
        get { return roomDimensions; }
    }
    private Vector3 roomOrigin = Vector3.zero;

    private bool useCenterAsOrigin = true;

    public Vector3 meshOrigin
    {
        get
        {
            if (useCenterAsOrigin)
            {
                return -roomDimensions * pivotOffsetFactor;
            }
            else
            {
                return Vector3.zero;
            }
        }
    }

    public float pivotOffsetFactor
    {
        get
        {
            if (useCenterAsOrigin)
            {
                return 0.5f;
            }
            else
            {
                return 1;
            }
        }
    }

    public float WidthEndPoint
    {
        get { return roomDimensions.x * pivotOffsetFactor; }
    }

    public float DepthEndPoint
    {
        get { return roomDimensions.z * pivotOffsetFactor; }
    }

    public float HeightEndPoint
    {
        get { return roomDimensions.y * pivotOffsetFactor; }
    }

    public float WidthStartPoint
    {
        get { return - roomDimensions.x * pivotOffsetFactor; }
    }

    public float DepthStartPoint
    {
        get { return - roomDimensions.z * pivotOffsetFactor; }
    }

    public float HeightStartPoint
    {
        get { return -roomDimensions.y * pivotOffsetFactor; }
    }
}