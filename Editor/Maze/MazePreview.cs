using Assets.SNEED.EditorExtensions.Maze;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Assets.SNEED.EditorEditorExtensions.Maze
{
    public class MazePreview
    {
        bool showSchematic = false;
        bool showModel = true;

        GameObject previewCamProxy;
        Camera previewCam;

        RenderTexture rt;
        Texture preview;

        //internal void RenderPreviewGUI(Rect r, Event current, GUIStyle background)
        //{
        //    if (showModel)
        //    {
        //        mazeInspector.RenderDefaultPreview(r, background);
        //        return;
        //    }

        //}

        internal void RenderPreviewSettings(Event current)
        {
            EditorGUILayout.BeginHorizontal();

            showModel = GUILayout.Toggle(showModel, showModelContent);
            showSchematic = GUILayout.Toggle(showSchematic, showSchematicContent);

            EditorGUILayout.EndHorizontal();
        }


        private MazeInspector mazeInspector;
        private beMobileMaze mazeToPreview;

        public MazePreview(beMobileMaze maze, RenderTexture renderTex = null)
        {
            mazeToPreview = maze;
            

        }

        public void SetupRenderTexture(RenderTexture renderTex = null)
        {

            if (renderTex == null)
            {
                rt = new RenderTexture(256, 256, 0, RenderTextureFormat.ARGB32);
                rt.name = "PreviewTexture";
                preview = rt;
            }
            else
            {
                preview = renderTex;
            }

        }


        public void ConfigureWithCurrentSceneViewCam()
        {
            previewCam = SceneView.lastActiveSceneView.camera;
            Handles.DrawCamera(new Rect(0, 0, 100, 100), SceneView.lastActiveSceneView.camera);
            previewCamProxy = GameObject.Instantiate(previewCam.gameObject);
            previewCam = previewCamProxy.GetComponent<Camera>();

            previewCamProxy.hideFlags = HideFlags.HideAndDontSave;
            //previewCam.renderingPath = RenderingPath.Forward;

            previewCam.cameraType = CameraType.SceneView;
            previewCam.clearFlags = CameraClearFlags.SolidColor;
            previewCam.orthographic = true;
            
            previewCam.orthographicSize = 20;
            previewCamProxy.transform.position = mazeToPreview.transform.position + (mazeToPreview.transform.up * 10);

            previewCamProxy.transform.LookAt(mazeToPreview.transform);
        }

        internal void Render()
        {
            previewCam.targetTexture = rt;
            previewCam.Render();

        }

        internal void Render(Rect previewTargetArea)
        {
            PreviewRenderUtility util = new PreviewRenderUtility(true);
            util.BeginPreview(previewTargetArea, null);

            Handles.CubeCap(0, Vector3.zero, Quaternion.identity, 100);

            preview = util.EndPreview();
        }


        public Texture GetPreviewImage()
        {
            if (preview != null)
                return preview;

            return Texture2D.whiteTexture;
        }

        public void cleanUp()
        {
            Editor.DestroyImmediate(previewCamProxy);
            Editor.DestroyImmediate(previewCam);
            Editor.DestroyImmediate(preview);
            Editor.DestroyImmediate(rt);
        }





        #region GUIContents

        private GUIContent showModelContent = new GUIContent("Model");
        private GUIContent showSchematicContent = new GUIContent("Schematic");
        public bool hasPreviewRendered { get { return preview != null; } }

        #endregion
    }
}
