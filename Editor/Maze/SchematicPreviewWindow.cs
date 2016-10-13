using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Assets.SNEED.EditorExtensions.Maze
{
    public class SchematicPreviewWindow : EditorWindow
    {
        PreviewRenderUtility previewUtil;
        Rect targetRect;
        Texture schema;
        beMobileMaze currentMaze;
        Camera schemaCam;
        EditorState state;
        static SchematicPreviewWindow window;

        public GameObject previewOrigin;

        private Texture renderSchematicPreview()
        {
            var cam = previewUtil.m_Camera;
            cam.orthographic = true;
            cam.transform.position = cam.transform.up * -5;
            cam.transform.LookAt(Vector3.zero);

            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);

            previewUtil.BeginStaticPreview(targetRect);
            
            previewUtil.DrawMesh(cube.GetComponent<MeshFilter>().sharedMesh, 
                Vector3.zero, Quaternion.identity, 
                cube.GetComponent<MeshRenderer>().sharedMaterial, 0);
            
            cam.Render();
            //Handles.SetCamera(cam);
            //Handles.Label(Vector3.zero, "Test " + currentMaze.name);
            //Handles.SphereCap(0, Vector3.zero, Quaternion.identity, 4);
            
            var result = previewUtil.EndStaticPreview();
            DestroyImmediate(cube);
            return result;
        }

        GameObject previewCamHost;
        internal void Initialize(beMobileMaze selectedMaze, EditorState state)
        {
            this.state = state;
            state.shouldRenderSchema = true;
            updateToNewMaze(selectedMaze);

            previewOrigin = EditorUtility.CreateGameObjectWithHideFlags("PreviewOrigin", HideFlags.HideAndDontSave);
            previewOrigin.transform.Translate(1000, 1000, 1000);

            previewCamHost = EditorUtility.CreateGameObjectWithHideFlags("SchemaCam", HideFlags.HideAndDontSave, typeof(Camera));

            schemaCam = previewCamHost.GetComponent<Camera>();

            schemaCam.cameraType = CameraType.SceneView;
            schemaCam.orthographic = true;
            schemaCam.orthographicSize = 10;
            previewCamHost.transform.position = previewOrigin.transform.position + previewOrigin.transform.up * 5;
            previewCamHost.transform.LookAt(previewOrigin.transform);

            state.schemaCam = schemaCam;
        }
        private void updateToNewMaze(beMobileMaze maze)
        {
            currentMaze = maze;
        }

        #region EditorWindow Messages
        void Awake()
        {
            previewUtil = new PreviewRenderUtility();
            Debug.Log("Awake on Schema Window");

        }

        void OnGUI()
        {
            targetRect = this.position;


            if (currentMaze == null)
                return;

            //if(schema == null)
            //{
            //    schema = renderSchematicPreview();
            //}


            if (Event.current.type == EventType.Repaint)
            {
                GUI.Box(new Rect(Vector2.zero, position.size),"");

                Handles.CubeCap(0, new Vector3(position.size.x * 0.5f, position.size.y * 0.5f), Quaternion.identity, 10000);
                // Handles.SetCamera(state.schemaCam);
                // Handles.SetCamera(Camera.current);
               Handles.DrawLine(new Vector2(0,0), targetRect.size);
                //Handles.DrawCamera(targetRect, state.schemaCam, DrawCameraMode.Textured);
               // Handles.DrawCamera(targetRect, Camera.current, DrawCameraMode.Textured);

            }
        }

        void OnEnable()
        {
            window = this;
        }
        void OnDisable()
        {
            state.shouldRenderSchema = false;
            window = null;

            var allCams = FindObjectsOfType<Camera>();
            var allSceneCams = SceneView.GetAllSceneCameras();
        }

        void OnSelectionChange()
        {
            var maze = Selection.activeGameObject.GetComponent<beMobileMaze>();
            if (maze != null)
            {
                updateToNewMaze(maze);
            }
        }

        #endregion

        [DrawGizmo(GizmoType.NonSelected)]
        static void RenderCustomGizmo(Transform t, GizmoType gizmoType)
        {
            if (window == null)
                return;

            //var maze = window.state.SelectedMaze;
            //var mazeTransform = maze.transform;

            var origin = window.previewOrigin.transform;
            Handles.Label(origin.position, "Preview Origin");

            //Handles.CubeCap(0, origin.position + new Vector3(0, 1), Quaternion.identity, 1);
            Handles.DrawLine(origin.position + new Vector3(0, 1),origin.position + new Vector3(0, 3));

        }

    }
}
