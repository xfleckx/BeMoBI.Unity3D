using UnityEngine;
using UnityEditor;
using System;
using Assets.SNEED.EditorExtensions;
using Assets.SNEED.Mazes;
using System.Linq;
using Assets.SNEED.EditorExtensions.Maze.UnitCreation;

namespace Assets.SNEED.EditorExtension.Maze
{
    public interface IEditorMode
    {
        string Name { get; }
        bool Selected { get; set; }
        
        void OnGUI(MazeCreationWorkflowBackEnd backend);
        void OnSceneViewGUI(SceneView view, MazeCreationWorkflowBackEnd backend, EditorViewGridVisualisation visual);
        Color GetPrimaryColor();
        Color GetErrorColor();
        void ProcessEvent(Event current, MazeCreationWorkflowBackEnd backend, EditorViewGridVisualisation visual);

        void GizmoDrawings(beMobileMaze maze, GizmoType type);
        void Reset();
    }

    public abstract class EditorMode : IEditorMode
    {
        protected MazeCreationWorkflowBackEnd backend;

        protected Color tileHighlighting = Color.gray;

        protected bool dragging = false;

        private int controlId = 0;

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
         
        public virtual void OnGUI(MazeCreationWorkflowBackEnd backend)
        {
            EditorGUILayout.HelpBox(Name, MessageType.Info);
        }

        public virtual void OnSceneViewGUI(SceneView view, MazeCreationWorkflowBackEnd backend, EditorViewGridVisualisation visual)
        {
            visual.pushHandleColor();

            Handles.color = tileHighlighting;
            
            visual.popHandleColor();
        }

        public virtual void GizmoDrawings(beMobileMaze maze, GizmoType type)
        {

        }

        public abstract Color GetPrimaryColor();

        public virtual Color GetErrorColor()
        {
            return Color.red;
        }

        public virtual void ProcessEvent(Event evt, MazeCreationWorkflowBackEnd backend, EditorViewGridVisualisation visual)
        {
            if (backend == null)
               Debug.Log("backend null");

            this.backend = backend;

            controlId = GUIUtility.GetControlID(FocusType.Passive);

            if (evt.type == EventType.MouseDown)
            {
                if (dragging)
                    DragEnds();

                Click(evt, evt.button);
            }

            if (evt.type == EventType.MouseDrag)
            {
                Drag(evt, evt.button);
            }

            if (evt.type == EventType.MouseUp)
            {
                if (dragging)
                    DragEnds();

                Click(evt, evt.button);
            }
        }

        protected virtual void DragEnds()
        {
            dragging = false;
        }

        protected virtual void Consume(Event evt)
        {
            GUIUtility.hotControl = controlId;
            evt.Use();
        }

        protected abstract void Click(Event evt, int button);
        protected abstract void Drag(Event evt, int button);

        public abstract void Reset();

    }
    
}