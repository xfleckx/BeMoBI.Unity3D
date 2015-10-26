using Assets.BeMoBI.Unity3D.Editor.Maze.UnitCreation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Assets.BeMoBI.Unity3D.Editor.Maze.UnitCreation
{
    public class HidingSpotCreator : CreatorState
    {
        Vector3 socketOffset;
        float socketHeight = 0.55f;
        float doorOffset = -0.3f;
        GameObject socketPrefab;
        bool useCustomSocket = false;

        public override string CreatorName
        {
            get
            {
                return "Hiding Spot";
            }
        }

        public override void Initialize()
        { 
            // for this unit it does nothing...
        }

        public override Rect OnGUI()
        {
            var rect = EditorGUILayout.BeginVertical();

                dimension = EditorGUILayout.Vector2Field("Spot Door dimension (width x height)", dimension);
                doorOffset = EditorGUILayout.FloatField("Door position", doorOffset);
                socketOffset = EditorGUILayout.Vector3Field("Socket position", socketOffset);
                socketHeight = EditorGUILayout.FloatField("Socket height", socketHeight);

                socketPrefab = EditorGUILayout.ObjectField("Custom socket:", socketPrefab, typeof(GameObject), false, null) as GameObject;

                useCustomSocket = EditorGUILayout.Toggle("Use custom socket", useCustomSocket) && (socketPrefab != null);

                if (GUILayout.Button("Create Hiding Spot", GUILayout.Height(25f)))
                {
                   constructedUnit = Construct();
                }

                EditorGUILayout.Space();

                Render_SaveAsPrefab_Option();

                pivotOrigin = EditorGUILayout.Vector3Field("Pivot Origin", pivotOrigin);

            EditorGUILayout.EndVertical();

            return rect;
        }

        protected override void OnBeforeCreatePrefab()
        {
            // save meshes as Assets

        }

        private GameObject Construct()
        {
            var spotHost = new GameObject("HidingSpot");
            
            var hidingSpotController = spotHost.AddComponent<HidingSpot>();
            
            var prototypeMaterial = GetPrototypeMaterial();
            
            float doorPanelWidth = dimension.x / 2f;
            float doorPanelHeight = dimension.y;

            var leftDoorMesh = CreateDoorPlane(doorPanelWidth, doorPanelHeight, Vector3.zero, Vector3.left);
            var rightDoorMesh = CreateDoorPlane(doorPanelWidth, doorPanelHeight, Vector3.zero, Vector3.right);

            var door = new GameObject("Door");
            door.transform.parent = spotHost.transform;
            door.transform.localPosition = new Vector3(0, 0, doorOffset);

            var doorCollider = door.AddComponent<BoxCollider>();

            doorCollider.center = new Vector3(0, Dimension.y / 2, 0.001f);

            doorCollider.size = Dimension;

            var panelLeft = new GameObject("Left_Panel");
            panelLeft.transform.parent = door.transform;
            AddMesh(panelLeft, leftDoorMesh, prototypeMaterial);
            panelLeft.transform.localPosition = new Vector3(dimension.x / 2, 0, 0);

            var panelRight = new GameObject("Right_Panel");
            panelRight.transform.parent = door.transform;
            AddMesh(panelRight, rightDoorMesh, prototypeMaterial);
            panelRight.transform.localPosition = new Vector3(-dimension.x / 2, 0, 0);

            hidingSpotController.roomSize = Dimension;
            hidingSpotController.DoorA = panelLeft;
            hidingSpotController.DoorB = panelRight;
            hidingSpotController.DoorMovingDirection = HidingSpot.Direction.Horizontal;

            var socket = GameObject.CreatePrimitive(PrimitiveType.Cube);
            socket.transform.parent = spotHost.transform;
            socket.transform.localPosition = socketOffset;
            socket.transform.localScale = new Vector3(0.2f, socketHeight, 0.2f);

            return spotHost;
        }

        private void AddMesh(GameObject host, Mesh mesh, Material material)
        {
            var filter = host.AddComponent<MeshFilter>();

            var renderer = host.AddComponent<MeshRenderer>();

            filter.mesh = mesh;

            renderer.material = material;
        }

        private Mesh CreateDoorPlane(float width, float height, Vector3 origin, Vector3 orientationFormOrigin)
        {
            var frontSize = V(width * orientationFormOrigin.x, height, 0);

            var mesh = new Mesh();

            if (orientationFormOrigin == Vector3.right) { 

                var vertices = new List<Vector3>()
                {
                    V(origin.x,     origin.y,       0),
                    V(origin.x,     frontSize.y,    0),
                    V(frontSize.x,  frontSize.y,    0),
                    V(frontSize.x,  origin.y,       0)
                };

                mesh.SetVertices(vertices);

                var triangles = new List<int>()
                {
                    2,3,0,
                    0,1,2
                };

                mesh.SetTriangles(triangles, 0);

            }
            else
            {
                var vertices = new List<Vector3>()
                {
                    V(frontSize.x,  frontSize.y,    0),
                    V(frontSize.x,  origin.y,       0),
                    V(origin.x,     origin.y,       0),
                    V(origin.x,     frontSize.y,    0)
                };

                mesh.SetVertices(vertices);

                var triangles = new List<int>()
                {
                    2,1,0,
                    0,3,2
                };

                mesh.SetTriangles(triangles, 0); 
            }

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
    }

}
