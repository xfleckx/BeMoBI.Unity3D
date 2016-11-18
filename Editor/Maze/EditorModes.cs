using UnityEngine;
using UnityEditor;
using System;
using Assets.SNEED.EditorExtensions;
using Assets.SNEED.Mazes;

namespace Assets.SNEED.EditorExtension.Maze
{
    public interface IEditorMode
    {
        string Name { get; }
        bool Selected { get; set; }
        
        void OnGUI();
        void OnSceneViewGUI(SceneView view, MazeCreationWorkflowBackEnd backend, EditorViewGridVisualisation visual);
        Color GetPrimaryColor();
        Color GetErrorColor();
    }

    public abstract class EditorMode : IEditorMode
    {
        protected MazeCreationWorkflowBackEnd backend;

        protected Color tileHighlighting = Color.gray;

        public abstract string Name
        {
            get;
            
        }

        private bool selected = false;
        public bool Selected
        {
            get
            {
                return selected;
            }

            set
            {
                selected = value;
            }
        }
         
        public void OnGUI()
        {
            EditorGUILayout.HelpBox(Name, MessageType.Info);
        }

        public void OnSceneViewGUI(SceneView view, MazeCreationWorkflowBackEnd backend, EditorViewGridVisualisation visual)
        {
            visual.pushHandleColor();

            Handles.color = tileHighlighting;
            
            visual.popHandleColor();
        }

        public abstract Color GetPrimaryColor();

        public virtual Color GetErrorColor()
        {
            return Color.red;
        }
    }


    public class DrawingMode : EditorMode
    {
        public override string Name
        {
            get
            {
              return "Draw or Wipe Units";
            }
        }

        public override Color GetPrimaryColor()
        {
            return Color.magenta;
        }
    }

    public class MergeAndSeparateMode : EditorMode
    {
        public override string Name
        {
            get
            {
                return "Connect Units";
            }
        }

        public override Color GetPrimaryColor()
        {
            return Color.blue;
        }
    }


    public class PathCreationMode : EditorMode
    {
        public override string Name
        {
            get
            {
                return "Create Paths";
            }
        }


        public override Color GetPrimaryColor()
        {
            return Color.yellow;
        }
    }
}