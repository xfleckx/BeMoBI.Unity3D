using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System;
using System.IO;

public class UnitCreator : EditorWindow
{ 
    [MenuItem("BeMoBI/Maze/Unit Creator")]
    static void Init()
    {
        var window = EditorWindow.GetWindow<UnitCreator>();

        window.titleContent = new GUIContent("Unit Creator");
        
        window.Initialize();

        window.Show();
    }


    List<String> expectedChildComponents;

    private Vector3 roomDimensions = Vector3.one;
    private Vector3 roomOrigin = Vector3.zero;

    private bool useCenterAsOrigin = false;
    private bool addBoxCollider = true;

    private Vector3 meshOrigin
    {
        get
        {
            if (useCenterAsOrigin)
            {
                return - roomDimensions / 2;
            }
            else
            {
                return Vector3.zero;
            }
        }
    }

    private float pivotOffsetFactor
    {
        get{
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

    private float meshWidthEndPoint
    {
        get { return roomDimensions.x * pivotOffsetFactor; }
    }

    private float meshDepthEndPoint
    {
        get { return roomDimensions.z * pivotOffsetFactor; }
    }

    private float meshHeightEndPoint
    {
        get { return roomDimensions.y * pivotOffsetFactor; }
    }

    private string prefabName = "MazeUnit";

    private GameObject constructedUnit;

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

        if (GUILayout.Button("Create new Maze Unit (Room)", GUILayout.Height(35f)))
        {
           constructedUnit = ConstructUnit(roomOrigin, roomDimensions);
        }

        if (GUILayout.Button("Resize Unit (Room)"))
        {
            
        }

        constructedUnit = EditorGUILayout.ObjectField("Created Unit", constructedUnit, typeof(GameObject), true) as GameObject;

        EditorGUILayout.Space();

        prefabName = EditorGUILayout.TextField("Prefab Name:", prefabName);

        if (constructedUnit != null && GUILayout.Button("Save as Prefab"))
        {
            var targetPath = EditorEnvironmentConstants.Get_PREFAB_DIR_PATH();

            Debug.Assert(AssetDatabase.IsValidFolder(targetPath), string.Format("Expected prefab folder at \"{0}\"", targetPath));

            var targetFilePath = targetPath + Path.AltDirectorySeparatorChar + string.Format("{0}{1}", prefabName, EditorEnvironmentConstants.PREFAB_EXTENSION);

            PrefabUtility.CreatePrefab(targetFilePath, constructedUnit);
        }

        EditorGUILayout.EndVertical();
    }



    private GameObject ConstructUnit(Vector3 origin, Vector3 dimension)
    {
        var newUnit = new GameObject("MazeUnit");

        newUnit.AddComponent<MazeUnit>();
        var boxCollider = newUnit.AddComponent<BoxCollider>();

        boxCollider.center = new Vector3(0, roomDimensions.y / 2, 0);
        
        boxCollider.size = roomDimensions;

        var prototype = GameObject.CreatePrimitive(PrimitiveType.Plane);
        prototype.hideFlags = HideFlags.HideAndDontSave;
        var prototypeMeshRenderer = prototype.GetComponent<MeshRenderer>();

        foreach (var item in expectedChildComponents)
        {
            var subComponent = new GameObject(item);
            
            subComponent.transform.parent = newUnit.transform;
            
            var meshFilter = subComponent.AddComponent<MeshFilter>();

            var meshRenderer = subComponent.AddComponent<MeshRenderer>();

            meshRenderer.material = prototypeMeshRenderer.sharedMaterial;

            if (item.Equals(MazeUnit.TOP))
            {
                var topMesh = CreateTopMesh();

                meshFilter.mesh = topMesh;


               // SaveIfNotAlreadyExisting(topMesh, )

                subComponent.transform.localPosition = V(0, roomDimensions.y, 0);
            }

            if (item.Equals(MazeUnit.FLOOR))
            {
                meshFilter.mesh = CreateFloorMesh();
            }
            
            if (item.Equals(MazeUnit.NORTH))
            {
                meshFilter.mesh = CreateNorthMesh();
                subComponent.transform.localPosition = V(0, roomDimensions.y / 2, roomDimensions.z / 2);
            }

            if (item.Equals(MazeUnit.SOUTH))
            {
                meshFilter.mesh = CreateSouthMesh();
                subComponent.transform.localPosition = V(0, roomDimensions.y / 2, - roomDimensions.z / 2);
            }

            if (item.Equals(MazeUnit.WEST))
            {
                meshFilter.mesh = CreateWestMesh();
                subComponent.transform.localPosition = V(-roomDimensions.x / 2, roomDimensions.y / 2, 0);
            }

            if (item.Equals(MazeUnit.EAST))
            {
                meshFilter.mesh = CreateEastMesh();
                subComponent.transform.localPosition = V(roomDimensions.x / 2, roomDimensions.y / 2, 0);
            }
        }

        DestroyImmediate(prototype);

        return newUnit; 
    }

    private Mesh CreateFloorMesh()
    {
        var mesh = new Mesh();

        var vertices = new List<Vector3>()
        {
            V(meshOrigin.x,      0, meshOrigin.z), 
            V(meshWidthEndPoint, 0, meshOrigin.z),
            V(meshOrigin.x,      0, meshDepthEndPoint),
            V(meshWidthEndPoint, 0, meshDepthEndPoint)
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

        return mesh;
    }

    private Mesh CreateTopMesh()
    {
        var mesh = new Mesh();

        var vertices = new List<Vector3>()
        {
            V(meshOrigin.x,         0, meshOrigin.z),
            V(meshWidthEndPoint,    0, meshOrigin.z),
            V(meshOrigin.x,         0, meshDepthEndPoint),
            V(meshWidthEndPoint,    0, meshDepthEndPoint)
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

        return mesh;
    }

    private Mesh CreateNorthMesh()
    {
        var mesh = new Mesh();

        var vertices = new List<Vector3>()
        {
            V(meshOrigin.x,         meshOrigin.y,           0),
            V(meshWidthEndPoint,    meshOrigin.y,           0),
            V(meshOrigin.x,         meshHeightEndPoint,      0),
            V(meshWidthEndPoint,    meshHeightEndPoint,     0)
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

        return mesh;
    }

    private Mesh CreateSouthMesh()
    {
        var mesh = new Mesh();

        var vertices = new List<Vector3>()
        {
            V(meshOrigin.x,         meshOrigin.y,        0),
            V(meshWidthEndPoint,    meshOrigin.y,        0),
            V(meshOrigin.x,         meshHeightEndPoint,  0),
            V(meshWidthEndPoint,    meshHeightEndPoint,  0)
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

        return mesh;
    }

    private Mesh CreateWestMesh()
    {
        var mesh = new Mesh();

        var vertices = new List<Vector3>()
        {
            V(0,   meshOrigin.y,         meshOrigin.z),
            V(0,   meshHeightEndPoint,   meshOrigin.z),
            V(0,   meshOrigin.y,         meshDepthEndPoint),
            V(0,   meshHeightEndPoint,   meshDepthEndPoint)
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
            Vector3.forward,
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

        return mesh;
    }

    private Mesh CreateEastMesh()
    {
        var mesh = new Mesh();

        var vertices = new List<Vector3>()
        {
            V(0,   meshOrigin.y,         meshOrigin.z),
            V(0,   meshHeightEndPoint,   meshOrigin.z),
            V(0,   meshOrigin.y,         meshDepthEndPoint),
            V(0,   meshHeightEndPoint,   meshDepthEndPoint)
        };

        mesh.SetVertices(vertices);

        var triangles = new List<int>()
        {
            0,2,1,
            2,3,1
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

        return mesh;
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