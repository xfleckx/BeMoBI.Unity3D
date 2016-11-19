using UnityEngine;
using UnityEditor;
using Assets.SNEED.EditorExtensions.Maze.UnitCreation;
using Assets.SNEED.EditorExtensions;
using Assets.SNEED.Mazes;
using System.Linq;
using Assets.SNEED.EditorExtensions.Maze;

namespace Assets.SNEED.EditorExtension.Maze.EditorModes
{
    public class PathCreationMode : EditorMode
    {
        PathInMaze pathToEdit;
        int selected = 0;
        bool tilePositionIsValid = false;
        public override string Name
        {
            get
            {
                return "Edit Paths";
            }
        }
        
        public override Color GetPrimaryColor()
        {
            return Color.yellow;
        }

        public override void OnGUI(MazeCreationWorkflowBackEnd backend)
        {
            var pathController = backend.selectedMaze.GetComponent<PathController>();
            var pathIds = pathController.GetAvailablePathIDs();
            var options = pathIds.Select(i => string.Format("{0} - Path", i)).ToArray();

            selected = EditorGUILayout.Popup(selected, options);

            pathToEdit = pathController.Paths.First(p => p.ID == selected);

            pathToEdit = EditorGUILayout.ObjectField("Edit: ", pathToEdit, typeof(PathInMaze), allowSceneObjects:true) as PathInMaze;

        }

        public override void OnSceneViewGUI(SceneView view, MazeCreationWorkflowBackEnd backend, EditorViewGridVisualisation visual)
        {
            tilePositionIsValid = CheckIfTileIsValidPathElement(backend, visual);

            //if (!tilePositionIsValid)
            //{
            //    visual.pushHandleColor();

            //    Handles.color = Color.red;
            //    Handles.DrawWireCube(backend.visual.MarkerPosition + , backend.selectedMaze.RoomDimension * 0.8f);

            //    visual.popHandleColor();
            //}
        }

        private bool CheckIfTileIsValidPathElement(MazeCreationWorkflowBackEnd backend, EditorViewGridVisualisation visual)
        {
            var tilePosition = visual.currentTilePosition;

            if (!backend.selectedMaze.Units.Any((u) => u.GridID.Equals(tilePosition)))
                return false;

            if (pathToEdit.PathAsLinkedList.Count == 0)
                return true;

            var lastElement = pathToEdit.PathAsLinkedList.Last;

            var gridIdOfLastElement = lastElement.Value.Unit.GridID;

            if (tilePosition == gridIdOfLastElement)
                return false;

            if (lastElement.Previous != null)
            {

                var gridIdOfPreviousElement = lastElement.Previous.Value.Unit.GridID;

                if (gridIdOfPreviousElement == tilePosition)
                    return false;
            }

            var deltaBetweenLastAndCurrent = tilePosition - gridIdOfLastElement;
            var absDelta = deltaBetweenLastAndCurrent.SqrMagnitude();

            if (absDelta > 1)
                return false;

            return true;
        }

        private void Add(MazeUnit newUnit)
        {
            var newElement = new PathElement(newUnit);

            newElement = PathEditorUtils.GetElementType(newElement);

            var nr_el = pathToEdit.PathAsLinkedList.Count; // count all elements in the path to get the second last for turning calculation

            if (nr_el >= 1)
            {
                var previousElement = pathToEdit.PathAsLinkedList.Last.Value;

                if (nr_el >= 2)
                {
                    var secpreviousElement = pathToEdit.PathAsLinkedList.Last.Previous.Value;
                    newElement = PathEditorUtils.GetTurnType(newElement, previousElement, secpreviousElement);
                }
                else
                {
                    newElement.Turn = TurnType.STRAIGHT;
                }
            }

            pathToEdit.PathAsLinkedList.AddLast(newElement);
        }

        private void Remove(MazeUnit unit)
        {
            var elementToRemove = pathToEdit.PathAsLinkedList.First(e => e.Unit.Equals(unit));

            pathToEdit.PathAsLinkedList.Remove(elementToRemove);
        }

        public override void GizmoDrawings(beMobileMaze maze, GizmoType type)
        {
            var pathController = backend.selectedMaze.GetComponent<PathController>();
            var pathToRender = pathController.Paths.First(p => p.ID == selected);

            var hoveringDistance = new Vector3(0, maze.RoomDimension.y * 0.9f, 0);
             
            var temp = Gizmos.color;

            Gizmos.color = tilePositionIsValid ? Color.green : Color.red;

            Gizmos.DrawCube(
                backend.visual.MarkerPosition + new Vector3(0, maze.RoomDimension.y * 0.5f, 0), 
                maze.RoomDimension * 0.4f);

            Gizmos.color = temp;
            
            PathEditorUtils.RenderPathElements(maze, pathToRender, hoveringDistance, GetPrimaryColor());
        }

        public override void Reset()
        {
        }

        protected override void Click(Event evt, int button)
        {
        }

        protected override void Drag(Event evt, int button)
        {
        }
    }
}